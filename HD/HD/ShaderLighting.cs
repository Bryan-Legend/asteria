using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;

namespace HD
{
    public enum ShadowmapSize
    {
        Size128 = 6,
        Size256 = 7,
        Size512 = 8,
        Size1024 = 9,
    }

    public static class ShaderLighting
    {

        const int margin = 80;
        // shader will adjust to any size you give it, but larger values will be biased towards the lower right.
        const int blockSize = 8;

        private static bool uniformBlur = false;

        public static Effect ShadowEffect { get; set; }

        // TODO: remove some of these targets and reuse them

        // the texture that represents the occlusion texture that each light will sample from
        public static Texture2D OcclusionTexture { get; set; }
        // fsQ is only used for debug purposes, we can remove it after we're sure everything is working properly
        private static Rectangle fullScreenQuad;
        // the target used for the first step of the shadow map resolve, one can be shared for all lights as they're handled sequentially.
        private static RenderTarget2D distanceTarget;
        // the target that we use to distort the distanceTarget to make parallell reduction easier
        private static RenderTarget2D distortTarget;
        // the targets for the parallel reduction, we might be able to do this in a smarter way, but if performance is reasonable then no need to bother.
        // right now this is by far the slowest part of the operation.
        private static RenderTarget2D[] reductionTarget;
        // the is the actual two pixel wide shadow map that is used to find closest occluder in each direction.
        private static RenderTarget2D shadowMapTarget;
        // target for preblurred and attenulated shadows.
        private static RenderTarget2D lightTarget;
        // the near final target
        private static RenderTarget2D nearFinalLightTarget;
        // final target
        private static RenderTarget2D finalLightTarget;

        private static RenderTarget2D sceneTarget;
        // reasonable defaults for minimum and maximum blur
        private static float minBlur = 4.0f;
        private static float maxBlur = 8.0f;

        //size of light in uv space.
        private static Vector2 uvLightSize;

        static int width;
        static int height;
        static int targetsize;
        static int reductionChainCount;

        // these are for our calling our vertex shader, otherwise we'd just use spritebatch as we were before.
        static VertexPositionTexture[] verticies;
        static short[] indicies;

        public static Color GetSkyColor()
        {
            return Main.DayNightGradientData[(int)Main.Map.GameTime.TimeOfDay.TotalMinutes];
            //return Color.FromNonPremultiplied(0, 0, 0, 128);
        }

        // will need to call this any time screen resolution changes. Since our lights are completely resolution independent, these options only affect quality, and not size
        public static void Initialize(GraphicsDevice device, ShadowmapSize mapsize, ShadowmapSize depthsize)
        {

            // create our targets up front
            int ReductionChainCount = (int)mapsize;
            targetsize = 2 << (int)mapsize;
            // todo: figure out a good format
            distanceTarget = new RenderTarget2D(device, targetsize, targetsize, false, SurfaceFormat.HalfVector2, DepthFormat.None);
            distortTarget = new RenderTarget2D(device, targetsize, targetsize, false, SurfaceFormat.HalfVector2, DepthFormat.None);
            lightTarget = new RenderTarget2D(device, targetsize, targetsize);
            nearFinalLightTarget = new RenderTarget2D(device, targetsize, targetsize);
            finalLightTarget = new RenderTarget2D(device, targetsize, targetsize);
            // todo: make this use depthsize
            shadowMapTarget = new RenderTarget2D(device, 2, targetsize, false, SurfaceFormat.HalfVector2, DepthFormat.None);
            fullScreenQuad = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
            sceneTarget = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            //sceneTarget = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height);
            reductionChainCount = (int)mapsize;
            reductionTarget = new RenderTarget2D[reductionChainCount];
            for (int i = 0; i < reductionChainCount; i++)
            {
                reductionTarget[i] = new RenderTarget2D(device, 2 << i, targetsize, false, SurfaceFormat.HalfVector2, DepthFormat.None);
            }

            // create full screen quad with position and UV data for Vertex shader
            verticies = new VertexPositionTexture[]
            {
                new VertexPositionTexture(
                    // bottom right
                    new Vector3(1.0f, -1.0f, 0.0f),
                    new Vector2(1.0f, 1.0f)),
                    // bottom left
                new VertexPositionTexture(
                    new Vector3(-1.0f,-1.0f,0.0f),
                    new Vector2(0.0f,1.0f)),
                    // top left
                new VertexPositionTexture(
                    new Vector3(-1.0f,1.0f,0.0f),
                    new Vector2(0.0f,0.0f)),
                    // top right
                new VertexPositionTexture(
                    new Vector3(1.0f,1.0f,0.0f),
                    new Vector2(1.0f,0.0f))
            };

            indicies = new short[] { 0, 1, 2, 2, 3, 0 };

            // lets make it a bit less than quarter the size of the screen as a test
            // needs to be adjusted for aspect ratio

            width = (Main.ResolutionWidth / blockSize) + margin * 2;
            height = (Main.ResolutionHeight / blockSize) + margin * 2;

            //todo: make this easier to work with
            //0.184375
            float xSize = Main.ResolutionWidth / blockSize;
            float ySize = Main.ResolutionHeight / blockSize;
            xSize = (xSize + margin * 2);
            ySize = (ySize + margin * 2);
            float difference = ySize / xSize;
            float xDif = difference * 0.25f;

            Vector2 vDifference = new Vector2((float)width / (width - margin), (float)height / (height - margin));

            uvLightSize = new Vector2(xDif, 0.25f);
        }

        public static void Draw(GraphicsDevice device, SpriteBatch spriteBatch, Rectangle viewPort, RenderTarget2D scene)
        {
            HandleInput();

            var currentOffsetX = (Main.ScreenOffset.X / blockSize);
            var currentOffsetY = (Main.ScreenOffset.Y / blockSize);

            if (OcclusionTexture == null)
            {
                width = (Main.ResolutionWidth / blockSize) + margin * 2;
                height = (Main.ResolutionHeight / blockSize) + margin * 2;
                OcclusionTexture = new Texture2D(device, width, height, false, SurfaceFormat.Single);
            }

            CreateOcculsionMap(OcclusionTexture);


            //Texture2D lightTexture = new Texture2D(device, width, height, false, SurfaceFormat.HalfSingle);
            //lightTexture.SetData(shadingBuffer);

            // UV space light position set to mouse for now
            Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            Vector2 oMapSize = new Vector2(width, height);
            mousePos /= new Vector2(viewPort.Width, viewPort.Height);

            var lightCount = 0;

            // for some reason XNA refuses to do any non-opaque blending with un-normalized texture formats like vector2
            device.BlendState = BlendState.Opaque;

            device.SetRenderTarget(sceneTarget);
            device.Clear(Color.Black);

            // entity lighting
            //HalfSingle[] shadingBuffer = new HalfSingle[width * height];Main.Map.Entities
            foreach (var entity in Main.Map.Entities)
            {
                if (entity.Light.A > 0)
                {
                    var x = ((int)entity.Position.X - (currentOffsetX * blockSize)) / blockSize;
                    var y = ((int)entity.Position.Y - (currentOffsetY * blockSize)) / blockSize;
                    if (x >= 0 - margin && x < width && y >= 0 - margin && y < height)
                    {
                        lightCount++;

                        //shadingBuffer[x, y] = 1f - (placeable.Type.Light.A / 255f);
                        //shadingBuffer[(y * width) + x] = new HalfSingle(1.0f);
                        device.BlendState = BlendState.Opaque;
                        device.SetRenderTarget(distanceTarget);
                        device.Clear(Color.Black);

                        //Vector2 difference = new Vector2((float)width / (width - margin), (float)height / (height - margin));

                        var thisLightSize = uvLightSize * GetLightSize(entity.Light);

                        // start first step
                        ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["ComputeDistances"];
                        // the light size and position need to be specified in UV space, this allows us to simplify our pixel shaders.
                        ShadowEffect.Parameters["uvLightPos"].SetValue(new Vector2(((float)x + margin) / width, ((float)y + margin) / height));
                        ShadowEffect.Parameters["uvLightSize"].SetValue(thisLightSize);
                        ShadowEffect.Parameters["renderTargetSize"].SetValue(new Vector2(targetsize, targetsize));
                        ShadowEffect.Parameters["InputTexture"].SetValue(OcclusionTexture);
                        //ShadowEffect.Parameters["marginSize"].SetValue(width/(width-(margin*2)));
                        ShadowEffect.CurrentTechnique.Passes[0].Apply();


                        //ShadowEffect.Parameters["occlusionMapSize"].SetValue(oMapSize);
                        device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, verticies, 0, 4, indicies, 0, 2);

                        //distortion
                        device.SetRenderTarget(distortTarget);
                        device.Clear(Color.Black);
                        ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["Distort"];
                        ShadowEffect.Parameters["InputTexture"].SetValue(distanceTarget);
                        ShadowEffect.CurrentTechnique.Passes[0].Apply();
                        device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, verticies, 0, 4, indicies, 0, 2);


                        //reduction
                        int step = reductionChainCount - 1;
                        RenderTarget2D s = distortTarget;
                        RenderTarget2D d = reductionTarget[step];
                        ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["HorizontalReduction"];

                        while (step >= 0)
                        {
                            d = reductionTarget[step];

                            if (step != 0)
                            {
                                device.SetRenderTarget(d);
                                device.Clear(Color.Black);
                            }
                            else
                            {
                                device.SetRenderTarget(shadowMapTarget);
                                device.Clear(Color.Black);
                            }

                            ShadowEffect.Parameters["ReductionTexture"].SetValue(s);
                            Vector2 textureDim = new Vector2(1.0f / (float)s.Width, 1.0f / (float)s.Height);
                            ShadowEffect.Parameters["TextureDimensions"].SetValue(textureDim);
                            ShadowEffect.CurrentTechnique.Passes[0].Apply();
                            device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, verticies, 0, 4, indicies, 0, 2);
                            device.SetRenderTarget(null);

                            s = d;
                            --step;
                        }

                        // now we get to draw the light and shadows to the target
                        device.SetRenderTarget(lightTarget);
                        device.Clear(Color.Black);
                        ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["DrawShadows"];
                        ShadowEffect.Parameters["ShadowMapTexture"].SetValue(shadowMapTarget);
                        ShadowEffect.CurrentTechnique.Passes[0].Apply();
                        device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, verticies, 0, 4, indicies, 0, 2);


                        // if blur quality is an issue then we can do multiple smaller blurs, but state switch overhead is already really high.  We'll want to consult the profiler before we consider it.
                        // first pass of two pass blur,
                        // TODO: make blurs resolution independent.
                        device.SetRenderTarget(nearFinalLightTarget);
                        device.Clear(Color.Black);
                        if (uniformBlur)
                            ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["UniformBlurHorizontally"];
                        else
                            ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["GaussianBlurHorizontally"];
                        ShadowEffect.Parameters["minBlur"].SetValue(minBlur);
                        ShadowEffect.Parameters["maxBlur"].SetValue(maxBlur);
                        ShadowEffect.Parameters["ShadowBlurTexture"].SetValue(lightTarget);
                        ShadowEffect.CurrentTechnique.Passes[0].Apply();
                        device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, verticies, 0, 4, indicies, 0, 2);

                        // second pass with attenuation

                        device.SetRenderTarget(finalLightTarget);
                        device.Clear(Color.Black);
                        if (uniformBlur)
                            ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["UniformBlurVerticallyAndAttenuate"];
                        else
                            ShadowEffect.CurrentTechnique = ShadowEffect.Techniques["GaussianBlurVerticallyAndAttenuate"];
                        ShadowEffect.Parameters["ShadowBlurTexture"].SetValue(nearFinalLightTarget);
                        ShadowEffect.CurrentTechnique.Passes[0].Apply();
                        device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, verticies, 0, 4, indicies, 0, 2);

                        int recWidth = (int)((float)Main.ResolutionWidth * thisLightSize.X);
                        Rectangle lightVis = new Rectangle(x * blockSize - recWidth / 2, y * blockSize - recWidth / 2, recWidth, recWidth);


                        device.SetRenderTarget(sceneTarget);
                        //device.Clear(Color.Black);
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
                        spriteBatch.Draw(finalLightTarget, lightVis, entity.Light.SetAlpha(1));
                        spriteBatch.End();

                        // next we need to blur and blend with with scene lighting.

                    }
                }
            }

            device.SetRenderTarget(null);

            if (!OcclusionTexture.IsDisposed)
            {
                BlendState blendState = new BlendState();
                blendState.ColorSourceBlend = Blend.DestinationColor;
                blendState.ColorDestinationBlend = Blend.SourceColor;
                MouseState ms = Mouse.GetState();
                //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                //int recWidth = (int)((float)Main.ResolutionWidth * uvLightSize.X);
                //Rectangle lightVis = new Rectangle(ms.X - recWidth / 2, ms.Y - recWidth / 2, recWidth, recWidth);
                //spriteBatch.Draw(OcclusionTexture, fullScreenQuad, Color.White);
                spriteBatch.Draw(scene, Vector2.Zero, Color.White);
                //spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
                spriteBatch.Draw(sceneTarget, fullScreenQuad, Color.White);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.DrawString(Utility.Font, String.Format("{0}\nminBlur = {1}\nmaxBlur = {2}\n{3} blur\n{4} FPS\n{5} Lights", mousePos, minBlur, maxBlur, uniformBlur ? "Uniform " : "Gaussian", Main.FrameRate, lightCount), Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            device.SetRenderTarget(null);
        }

        static float GetLightSize(Color lightColor)
        {
            return lightColor.A / 100f;
        }

        private static void HandleInput()
        {
            const float delta = 0.05f;
            if (Main.CurrentKeyboard.IsKeyDown(Keys.OemOpenBrackets))
                minBlur = minBlur - delta >= 0 ? minBlur - 0.05f : 0;
            if (Main.CurrentKeyboard.IsKeyDown(Keys.OemCloseBrackets))
                minBlur += delta;
            if (Main.CurrentKeyboard.IsKeyDown(Keys.OemSemicolon))
                maxBlur = maxBlur - delta >= 0 ? maxBlur - 0.05f : 0;
            if (Main.CurrentKeyboard.IsKeyDown(Keys.OemQuotes))
                maxBlur += delta;
            if (Utility.IsPressed(Main.CurrentKeyboard, Main.LastKeyboard, Keys.OemQuestion))
                uniformBlur = !uniformBlur;

            if (Main.CurrentKeyboard.IsKeyDown(Keys.OemComma))
                uvLightSize *= 0.95f;
            if (Main.CurrentKeyboard.IsKeyDown(Keys.OemPeriod))
                uvLightSize *= 1.05f;
        }

        public static void CreateOcculsionMap(Texture2D Texture)
        {
            var currentOffsetX = (Main.ScreenOffset.X / blockSize);
            var currentOffsetY = (Main.ScreenOffset.Y / blockSize);
            var result = new float[height * width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xOffset = (currentOffsetX + x) * blockSize;
                    var yOffset = (currentOffsetY + y) * blockSize;

                    xOffset -= margin * blockSize;
                    yOffset -= margin * blockSize;

                    var material = Main.Map.GetMaterialAtPixel(xOffset, yOffset);
                    //var materialInfo = MaterialInfo.MaterialTypes[(byte)material];

                    if (MaterialInfo.IsGas(material))
                        result[(y * width) + x] = 0.0f;
                    else
                    {
                        if (MaterialInfo.IsLooseOrSolid(material))
                        {
                            result[(y * width) + x] = 0.8f;
                        }
                        else
                        {
                            result[(y * width) + x] = 0.9f;
                        }
                    }

                    // here's how to get the lighting information from the materials.
                    var materialInfo = MaterialInfo.MaterialTypes[(byte)material];
                    if (materialInfo.Light.A > 0)
                    {
                        //lights.Add(new Vector3((float)xOffset/1.0f, yOffset/1.0f, materialInfo.Light.A));
                    }
                }
            }

            Texture.SetData(result);
        }
    }
}