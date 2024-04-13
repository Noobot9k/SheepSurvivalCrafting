using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour {

    [HideInInspector]
    public static Crafting Current;

    public List<CraftingRecipe> Recipes = new List<CraftingRecipe>();

    public Item Craft(Item item1, Item item2) {
        if(item1 == null || item2 == null) return null;
        foreach(CraftingRecipe recipe in Recipes) {
            if((item1.DisplayID == recipe.Item1.DisplayID && item2.DisplayID == recipe.Item2.DisplayID) || (item2.DisplayID == recipe.Item1.DisplayID && item1.DisplayID == recipe.Item2.DisplayID))
                return recipe.ResultingItem;
        }
        return null;
    }
    void Start() {
        Current = this;
    }
    void Update() {

    }
}

[System.Serializable]
public class CraftingRecipe {
    public Item Item1;
    public Item Item2;
    public Item ResultingItem;
}
