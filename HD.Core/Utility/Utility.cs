using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Microsoft.Xna.Framework;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using System.Text.RegularExpressions;
using System.Reflection;
using Ionic.Zlib;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;

#if WINDOWS
namespace ParallelTasks { }
#endif

namespace HD
{
    public static class Utility
    {
        public static string GameName = "Asteria";
        public static uint SteamAppId = 307130;

#if WINDOWS
        public static string SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Path.Combine("My Games", GameName));
#else
        public static string SavePath = "";
        public static ContentManager Content;
#endif
        public static string ContentDirectory = Path.GetFullPath("Content");
        public const int LightingMargin = 50;

        public const float RightDirection = 0;
        public const float DownRightDirection = (float)(1 * (Math.PI / 4));
        public const float DownDirection = (float)Math.PI / 2;
        public const float DownLeftDirection = (float)(3 * (Math.PI / 4));
        public const float LeftDirection = (float)Math.PI;
        public const float UpLeftDirection = (float)(5 * (Math.PI / 4));
        public const float UpDirection = (float)(3 * (Math.PI / 2));
        public const float UpRightDirection = (float)(7 * (Math.PI / 4));

        static Random random = new Random(); // keep this private because it is not thread safe.
        static BitArray randomBits;
        static int randomBitIndex;
        const int randomBitSize = 512;

        public static Action<string> OnLogMessage;

        static string statusMessage;
        public static string StatusMessage
        {
            get { return statusMessage; }
            set
            {
                LogMessage(value);
                statusMessage = value;
            }
        }

        public static SpriteFont SmallFont;
        public static SpriteFont MediumFont;
        public static SpriteFont Font;
        public static Texture2D EmptySlotTexture { get; set; }
        public static Texture2D FilledSlotTexture;
        public static Texture2D PixelTexture { get; set; }
        public static Texture2D BrushTexture { get; set; }
        public static Effect BlendEffect { get; set; }
        public static Vector2 CursorPosition;
        static StreamWriter logStream;
        static bool isLogFileDisabled;

        public static string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        static Utility()
        {
            var bytes = new byte[randomBitSize];
            random.NextBytes(bytes);
            randomBits = new BitArray(bytes);

            //int falseCount = 0;
            //foreach (bool bit in randomBits)
            //{
            //    if (!bit)
            //        falseCount++;
            //}
            //Console.WriteLine(falseCount);
        }

        public static void LogMessage(string format, params object[] args)
        {
            LogMessage(String.Format(format, args));
        }

        public static void LogMessage(string message)
        {
#if WINDOWS
            if (logStream == null && !isLogFileDisabled) {
                Directory.CreateDirectory(Utility.SavePath);

                var filename = "MessageLog.txt";
                if (World.IsServer)
                    filename = "Server " + World.ServerPort + " " + filename;
                try {
                    var stream = new FileStream(Path.Combine(SavePath, filename), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    stream.Seek(0, SeekOrigin.End);
                    logStream = new StreamWriter(stream);
                } catch (IOException) {
                    isLogFileDisabled = true;
                }
            }

            message = DateTime.Now.ToString() + ": " + message;

            //System.Diagnostics.Debug.WriteLine(message);
            Console.WriteLine(message);

            if (logStream != null) {
                logStream.WriteLine(message);
                logStream.Flush();
            }
#else
            System.Diagnostics.Debug.WriteLine(message);
#endif

            if (OnLogMessage != null)
                OnLogMessage(message);
        }

        public static double NextDouble()
        {
            lock (random) {
                return random.NextDouble();
            }
        }

        public static double NextDoubleBalanced()
        {
            lock (random) {
                return random.NextDouble() - 0.5;
            }
        }

        public static int Next()
        {
            lock (random) {
                return random.Next();
            }
        }

        public static int Next(int exclusiveMaximum)
        {
            lock (random) {
                return random.Next(exclusiveMaximum);
            }
        }

        public static int Next(int inclusiveMaximum, int exclusiveMaximum)
        {
            lock (random) {
                return random.Next(inclusiveMaximum, exclusiveMaximum);
            }
        }

        public static bool Roll(int odds)
        {
            lock (random) {
                return random.Next(odds) == 0;
            }
        }

        public static bool Roll4()
        {
            return Flip() && Flip();
        }

        public static bool Roll8()
        {
            return Flip() && Flip() && Flip();
        }

        public static bool Roll16()
        {
            return Flip() && Flip() && Flip() && Flip();
        }

        public static bool Roll32()
        {
            return Flip() && Flip() && Flip() && Flip() && Flip();
        }

        public static bool Roll64()
        {
            return Flip() && Flip() && Flip() && Flip() && Flip() && Flip();
        }

        public static bool Roll128()
        {
            return Flip() && Flip() && Flip() && Flip() && Flip() && Flip() && Flip();
        }

        public static bool Flip()
        {
            randomBitIndex++;
            if (randomBitIndex >= (randomBitSize * 8) - 32) // don't hit the end to avoid threading issues
                randomBitIndex = 0;
            return randomBits[randomBitIndex];
        }

        // source: http://thedeadpixelsociety.com/2012/01/hex-colors-in-xna/
        /// Creates a <see cref="Color"/> value from an ARGB or RGB hex string.  The string may
        /// begin with or without the hash mark (#) character.
        /// </summary>
        /// <param name="hexString">The ARGB hex string to parse.</param>
        /// <returns>
        /// A <see cref="Color"/> value as defined by the ARGB or RGB hex string.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if the string is not a valid ARGB or RGB hex value.</exception>
        public static Color HexToColor(this string hexString)
        {
            if (hexString.StartsWith("#"))
                hexString = hexString.Substring(1);
            uint hex = uint.Parse(hexString, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            Color color = Color.White;
            if (hexString.Length == 8) {
                color.A = (byte)(hex >> 24);
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            } else if (hexString.Length == 6) {
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            } else {
                throw new InvalidOperationException("Invald hex representation of an ARGB or RGB color value.");
            }
            return color;
        }

        public static Color SetAlpha(this Color color, double alpha)
        {
            return color * (float)alpha;
            //if (alpha >= 1)
            //    return color;

            //if (alpha > 0)
            //    return Color.FromNonPremultiplied(color.R, color.G, color.B, (byte)(255 * alpha));

            //return Color.Transparent;
        }

        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static Vector2 Round(this Vector2 vector, int roundTo)
        {
            return new Vector2(((int)vector.X / roundTo) * roundTo, ((int)vector.Y / roundTo) * roundTo);
        }

        public static Vector2 Offset(this Vector2 vector, float amount, float angle)
        {
            return vector + Vector2.Transform(new Vector2(amount, 0), Matrix.CreateRotationZ(angle));
        }

        public static Vector2 Offset(this Vector2 vector, Point amount)
        {
            return vector + new Vector2(amount.X, amount.Y);
        }

        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(PixelTexture, rect, color);
        }

        public static Rectangle GetSprite8(int x, int y)
        {
            return new Rectangle(x * 9, y * 9, 8, 8);
        }

        public static Rectangle GetSprite16(int x, int y)
        {
            return new Rectangle(x * 17, y * 17, 16, 16);
        }

        public static Rectangle GetSprite32(int x, int y)
        {
            return new Rectangle(x * 33, y * 33, 32, 32);
        }

        public static bool IsShift(this KeyboardState current)
        {
            return current.IsKeyDown(Keys.LeftShift) || current.IsKeyDown(Keys.RightShift);
        }

        public static bool IsControl(this KeyboardState current)
        {
            return current.IsKeyDown(Keys.LeftControl) || current.IsKeyDown(Keys.RightControl);
        }

        public static bool IsAlt(this KeyboardState current)
        {
            return current.IsKeyDown(Keys.LeftAlt) || current.IsKeyDown(Keys.RightAlt);
        }

        public static bool IsPressed(this KeyboardState current, KeyboardState last, Keys keys)
        {
            return !last.IsKeyDown(keys) && current.IsKeyDown(keys);
        }

        public static bool IsPressed(this GamePadState current, GamePadState last, Buttons keys)
        {
            return !last.IsButtonDown(keys) && current.IsButtonDown(keys);
        }

#if WINDOWS
        static System.Security.Cryptography.SHA512 hasher = System.Security.Cryptography.SHA512.Create();
        public static string CalculateHash(string input)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes("s&~D$L{a8_" + input);
            byte[] hash = hasher.ComputeHash(inputBytes);
            return Convert.ToBase64String(hash);
        }

        public static System.Windows.Media.Imaging.BitmapSource OpenPng(string filename)
        {
            var fullFilename = Path.Combine(ContentDirectory, filename);
            if (!File.Exists(fullFilename))
                fullFilename = Path.Combine(SavePath, "Content", filename);
            if (!File.Exists(fullFilename))
                return null;

            using (var stream = File.OpenRead(fullFilename)) {
                var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat, System.Windows.Media.Imaging.BitmapCacheOption.Default);
                return decoder.Frames[0];
            }
        }
#endif

        public static Texture2D LoadTexture(GraphicsDevice device, string filename)
        {
#if WINDOWS
            var fullFilename = Path.Combine(ContentDirectory, filename);
            if (!File.Exists(fullFilename))
                fullFilename = Path.Combine(SavePath, Path.Combine("Content", filename));
            if (!File.Exists(fullFilename))
                return null;

            using (var stream = File.OpenRead(fullFilename)) {
                var result = Texture2D.FromStream(device, stream);
                if (filename.EndsWith(".png"))
                    result = result.PreMultiply();
                return result;
            }
#else
            filename = Path.ChangeExtension(filename, null);

            if (!File.Exists(Path.Combine(Content.RootDirectory, filename + ".xnb")))
                return null;

            return Content.Load<Texture2D>(filename);
#endif
        }

        // http://xboxforums.create.msdn.com/forums/p/108749/642089.aspx
        public static Texture2D PreMultiply(this Texture2D value)
        {
            Color[] data = new Color[value.Width * value.Height];
            value.GetData<Color>(data);
            for (int i = 0; i < data.Length; i++) {
                data[i].R = (byte)(data[i].R * (data[i].A / 255.0f));
                data[i].G = (byte)(data[i].G * (data[i].A / 255.0f));
                data[i].B = (byte)(data[i].B * (data[i].A / 255.0f));
            }
            value.SetData<Color>(data);
            return value;
        }

        public static SoundEffect LoadSoundEffect(string filename, out float volume)
        {
            volume = 1.0f;
            var soundPath = Path.Combine(ContentDirectory, "Sounds");

            foreach (var file in Directory.GetFiles(soundPath, filename + "*.wav")) {
                var filenameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                if (filenameWithoutExtension.Length != filename.Length) {
                    var nameParts = filenameWithoutExtension.Split(' ');
                    if (Single.TryParse(nameParts[nameParts.Length - 1], out volume))
                        volume /= 100;
                }

                using (var stream = File.OpenRead(file)) {
                    var result = SoundEffect.FromStream(stream);
                    return result;
                }
            }

            //Utility.LogMessage(filename + " not found");
            return null;
        }

        public static SoundEffect LoadSong(string filename)
        {
            var fullFilename = Path.Combine(ContentDirectory, Path.Combine("Music", filename + ".wav"));
            if (!File.Exists(fullFilename))
                fullFilename = Path.Combine(SavePath, Path.Combine("Content", Path.Combine("Music", filename + ".wav")));

            if (!File.Exists(fullFilename)) {
                Utility.LogMessage(filename + " song not found");
                return null;
            }

            using (var stream = File.OpenRead(fullFilename)) {
                var result = SoundEffect.FromStream(stream);
                return result;
            }
        }

        // http://www.hanselman.com/blog/ASmarterOrPureEvilToStringWithExtensionMethods.aspx
        public static string FormatWith(this object anObject, string aFormat, IFormatProvider formatProvider = null, Func<string, string> onMissingProperty = null)
        {
            StringBuilder sb = new StringBuilder();
            Type type = anObject.GetType();
            Regex reg = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(aFormat);
            int startIndex = 0;
            foreach (Match m in mc) {
                Group g = m.Groups[2]; //it's second in the match between { and }
                int length = g.Index - startIndex - 1;
                sb.Append(aFormat.Substring(startIndex, length));

                string toGet = String.Empty;
                string toFormat = String.Empty;
                int formatIndex = g.Value.IndexOf(":"); //formatting would be to the right of a :
                if (formatIndex == -1) //no formatting, no worries
                {
                    toGet = g.Value;
                } else //pickup the formatting
                {
                    toGet = g.Value.Substring(0, formatIndex);
                    toFormat = g.Value.Substring(formatIndex + 1);
                }

                //first try properties
                PropertyInfo retrievedProperty = type.GetProperty(toGet);
                Type retrievedType = null;
                object retrievedObject = null;
                if (retrievedProperty != null) {
                    retrievedType = retrievedProperty.PropertyType;
                    retrievedObject = retrievedProperty.GetValue(anObject, null);
                } else //try fields
                {
                    FieldInfo retrievedField = type.GetField(toGet);
                    if (retrievedField != null) {
                        retrievedType = retrievedField.FieldType;
                        retrievedObject = retrievedField.GetValue(anObject);
                    }
                }

                if (retrievedType != null) //Cool, we found something
                {
                    string result = String.Empty;
                    if (toFormat == String.Empty) //no format info
                    {
                        result = retrievedType.InvokeMember("ToString",
                          BindingFlags.Public | BindingFlags.NonPublic |
                          BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                          , null, retrievedObject, null) as string;
                    } else //format info
                    {
                        result = retrievedType.InvokeMember("ToString",
                          BindingFlags.Public | BindingFlags.NonPublic |
                          BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                          , null, retrievedObject, new object[] { toFormat, formatProvider }) as string;
                    }
                    sb.Append(result);
                } else //didn't find a property with that name, so be gracious and put it back
                {
                    if (onMissingProperty != null)
                        sb.Append(onMissingProperty(g.Value));
                    else {
                        sb.Append("{");
                        sb.Append(g.Value);
                        sb.Append("}");
                    }
                }
                startIndex = g.Index + g.Length + 1;
            }
            if (startIndex < aFormat.Length) //include the rest (end) of the string
            {
                sb.Append(aFormat.Substring(startIndex));
            }
            return sb.ToString();
        }

        public static int GetTierAmount(int tier, double factor)
        {
            if (tier <= 1)
                tier = 1;
            return (int)(factor * Math.Pow(2, tier - 1));
        }

        public static Vector2 WriteLine(this SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color = default(Color))
        {
            if (color == default(Color))
                color = Color.White;
            var textSize = font.MeasureString(text);
            batch.DrawString(font, text, position, color);
            position.Y += textSize.Y;
            return position;
        }

        public static float WriteLine(this SpriteBatch batch, SpriteFont font, string text, Color color = default(Color))
        {
            if (text == null || text.Length == 0)
                return 0;
            if (color == default(Color))
                color = Color.White;
            var textSize = font.MeasureString(text);
            batch.DrawString(font, text, CursorPosition, color);
            CursorPosition.Y += textSize.Y;
            return textSize.X;
        }

        public static void DrawRightAlignedText(this SpriteBatch batch, SpriteFont font, string text, Vector2 position)
        {
            var textSize = font.MeasureString(text);
            position.X -= textSize.X;
            batch.DrawString(font, text, position, Color.White);
        }

        public static void DrawExpandButtonTexture(this SpriteBatch batch, Texture2D texture, Vector2 position, int width, Color color = default(Color), int margin = 8)
        {
            if (color == default(Color))
                color = Color.White;
            batch.Draw(texture, position, new Rectangle(0, 0, margin, texture.Height), color);
            batch.Draw(texture, new Rectangle((int)position.X + margin, (int)position.Y, width - margin - margin, texture.Height), new Rectangle(margin, 0, 1, texture.Height), color);
            batch.Draw(texture, position + new Vector2(width - margin, 0), new Rectangle(texture.Width - margin, 0, margin, texture.Height), color);
        }


        /// <summary>
        /// Generates a random angle.
        /// </summary>
        /// <param name="degrees">Max angle offset in degrees</param>
        /// <returns>Random angle in radians.</returns>
        public static float RandomAngle(int degrees = 360)
        {
            lock (random) {
                return (float)((random.NextDouble() - 0.5) * ((Math.PI / 180) * degrees));
            }
        }

        public static void DrawBlend(SpriteBatch batch, Texture2D primary, Texture2D secondary, float amount, Vector2 position)
        {
            if (primary == null)
                return;

            // since the first register gets filled with whatever you send to the draw method, we really only need to set
            // the second one as a parameter. 
            BlendEffect.Parameters["xTexture2"].SetValue(secondary);

            // Optionally we can set our sampling rules, but I've just left them default for now and hard
            // coded them into the shader. If at some point you decide you want the "minecraftey pixelated look" on something,
            // you can change the magnification filters to "POINT" Your texels will have to be larger than output pixels for the effect to show, though.

            // set the value in the shader
            BlendEffect.Parameters["xBlendAmount"].SetValue(amount);

            batch.Draw(primary, position, Color.White);
        }

        public static string WrapText(this SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');

            StringBuilder sb = new StringBuilder();

            float lineWidth = 0f;

            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words) {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth) {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                } else {
                    if (sb.Length > 0)
                        sb.Append("\n" + word + " ");
                    else
                        sb.Append(word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        public static Vector2 RandomVector()
        {
            lock (random) {
                return Vector2.Transform(new Vector2((float)Utility.random.NextDouble() - 0.5f, 0), Matrix.CreateRotationZ(RandomAngle()));
            }
        }

        static Color[] tierColors = {
                                 Color.FromNonPremultiplied(61, 247, 251, 128), // iron
                                 Color.FromNonPremultiplied(246, 214, 251, 128), // steel
                                 Color.FromNonPremultiplied(255, 254, 60, 128), // copper
                                 Color.FromNonPremultiplied(173, 255, 255, 128), // silver
                                 Color.FromNonPremultiplied(255, 115, 14, 128), // gold
                                 Color.FromNonPremultiplied(95, 72, 255, 128), // darksteel
                                 Color.FromNonPremultiplied(91, 255, 68, 128), // adamantium
                                 Color.FromNonPremultiplied(255, 215, 57, 128), // Obsidian
                                 Color.FromNonPremultiplied(255, 152, 255, 128), // Einsteinium
                             };
        public static Texture2D MissingTexture;

        public static Color GetTierColor(int tier)
        {
            if (tier < 1 || tier > 9)
                return Color.White;
            return tierColors[tier - 1];
        }

        public static Color GetColor(this Rarity rarity)
        {
            switch (rarity) {
                case Rarity.Poor:
                    return Color.LightGray;
                case Rarity.Common:
                    return Color.White;
                case Rarity.Uncommon:
                    return Color.LimeGreen;
                case Rarity.Rare:
                    return Color.DeepSkyBlue;
                case Rarity.Epic:
                    return Color.Orchid;
                case Rarity.Heroic:
                    return Color.Crimson;
                default:
                    return Color.White;
            }
        }

        public static float GetAngle(this Vector2 target)
        {
            return (float)Math.Atan2(target.Y, target.X);
        }

        public static string AddSpaces(this string source)
        {
            return Regex.Replace(source, "(\\B[A-Z0-9])", " $1");
        }

        public static float NormalizeAngle(float angle)
        {
            if (angle < 0 || angle > Math.PI * 2)
                return Math.Abs(((float)Math.PI * 2) - Math.Abs(angle));
            else
                return angle;
        }

        public static int Oscillate(int current, int frames)
        {
            // frames = 5
            // shows frames 0 1 2 3 4 3 2 1
            // 0-7

            // frames = 4
            // shows frames 0 1 2 3 2 1
            var result = current % ((frames - 1) * 2); // 0-5
            // oscillate backwards for 2 frames
            if (result > frames - 1)
                result = frames + frames - 2 - result;

            //Console.WriteLine(result);

            return result;
        }

        /// <summary>
        /// LongOscillate displays the end frame an extra time
        /// </summary>
        /// <param name="current"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static int LongOscillate(int current, int frames)
        {
            // frames = 5
            // shows frames 0 1 2 3 4 4 3 2 1
            // 0-8

            // frames = 4
            // shows frames 0 1 2 3 3 2 1
            var result = current % ((frames * 2) - 1); // 0-6
            if (result > frames - 1)
                result = frames + frames - 1 - result;

            //Console.WriteLine(result);

            return result;
        }

        public static string GetTierName(int tier)
        {
            switch (tier) {
                case 1:
                    return "Iron";
                case 2:
                    return "Steel";
                case 3:
                    return "Copper";
                case 4:
                    return "Silver";
                case 5:
                    return "Gold";
                case 6:
                    return "Darksteel";
                case 7:
                    return "Adamantium";
                case 8:
                    return "Obsidian";
                case 9:
                    return "Einsteinium";
                default:
                    return tier.ToString();
            }
        }

        public static Rectangle GetFrameSourceRectangle(int width, int height, int framesPerRow, int frame)
        {
            return new Rectangle((frame % framesPerRow) * width, (frame / framesPerRow) * height, width, height);
        }

        public static void CreateSaveGameDirectories()
        {
            Directory.CreateDirectory(Utility.SavePath);
            Directory.CreateDirectory(Path.Combine(Utility.SavePath, Path.Combine("Content", "Enemies")));
            Directory.CreateDirectory(Path.Combine(Utility.SavePath, Path.Combine("Content", "Plugins")));
            Directory.CreateDirectory(Path.Combine(Utility.SavePath, Path.Combine("Content", "Items")));
            Directory.CreateDirectory(Path.Combine(Utility.SavePath, Path.Combine("Content", "Music")));
            Directory.CreateDirectory(Path.Combine(Utility.SavePath, Path.Combine("Content", "Projectiles")));
        }

        // http://stackoverflow.com/questions/3278827/how-to-convert-a-structure-to-a-byte-array-in-c
        public static byte[] GetBytes(this GamePadState state)
        {
#if WINDOWS
            if (!state.IsConnected)
                return null;

            int size = Marshal.SizeOf(state);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(state, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
#else
            throw new NotImplementedException();
#endif
        }

        public static GamePadState GamePadStateFromBytes(byte[] bytes)
        {
#if WINDOWS
            var str = new GamePadState();

            if (bytes != null)
            {
                int size = Marshal.SizeOf(str);
                IntPtr ptr = Marshal.AllocHGlobal(size);

                Marshal.Copy(bytes, 0, ptr, size);

                str = (GamePadState)Marshal.PtrToStructure(ptr, str.GetType());
                Marshal.FreeHGlobal(ptr);
            }

            return str;
#else
            throw new NotImplementedException();
#endif
        }

        public static IEnumerable<T> GetEnumValues<T>()
        {
            var result = new List<T>();
            foreach (var fieldInfo in (typeof(T)).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                result.Add((T)Enum.Parse(typeof(T), fieldInfo.Name, true));
            }
            return result;
        }
    }
}