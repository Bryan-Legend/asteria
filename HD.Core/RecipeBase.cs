using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HD
{
    public static class RecipeBase
    {
        public static List<Recipe> Recipes;
        public static Dictionary<int, Recipe> RecipesById;
        static int nextRecipeId = 1;

        static RecipeBase()
        {
            Recipes = new List<Recipe>();
            RecipesById = new Dictionary<int, Recipe>();
        }

        public static void AddRecipe(Recipe recipe)
        {
            if (recipe.Id == 0)
                recipe.Id = nextRecipeId++;

            recipe.Creates.Amount = recipe.CreateAmount;

            RecipesById[recipe.Id] = recipe;
            Recipes.Add(recipe);
        }

        public static void Sort()
        {
            Recipes = Recipes.OrderBy(i => i.Creates.Type.Category).ThenBy(i => i.Creates.Type.ListPriority).ThenBy(i => i.Creates.Type.Tier).ThenBy(i => i.Creates.Type.Name).ToList();

            foreach (var recipe in Recipes)
            {
                foreach (var component in recipe.Components)
                {
                    if (component.Type.ComponentFor == null)
                        component.Type.ComponentFor = new List<string>();

                    if (!component.Type.ComponentFor.Contains(recipe.Creates.Type.Name)) {
                        component.Type.ComponentFor.Add(recipe.Creates.Type.Name);
                        component.Type.ComponentFor.Sort();
                    }
                }
            }
        }

        public static Recipe Get(int id)
        {
            return RecipesById[id];
        }
    }
}