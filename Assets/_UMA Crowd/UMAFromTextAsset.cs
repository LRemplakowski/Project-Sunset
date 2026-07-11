using System.Collections;
using Sirenix.OdinInspector;
using UMA.CharacterSystem;
using UnityEngine;

[RequireComponent(typeof(DynamicCharacterAvatar))]
public class UMAFromTextAsset : MonoBehaviour
{
    [SerializeField, Required]
    private TextAsset _umaTextAsset;
    [SerializeField]
    private DynamicCharacterAvatar _avatar;

    private void OnValidate()
    {
        if (_avatar == null)
        {
            _avatar = GetComponent<DynamicCharacterAvatar>();
        }
        _avatar.BuildCharacterEnabled = false;
        _avatar.preloadWardrobeRecipes.recipes.Clear();
    }

    [Button]
    private void CleanUMA()
    {
        _avatar.BuildCharacterEnabled = false;
        _avatar.preloadWardrobeRecipes.recipes.Clear();
    }

#if UNITY_EDITOR
    [Button]
    private void LoadRecipe()
    {
        _avatar.BuildCharacterEnabled = true;
        _avatar.BuildCharacterEnabled = false;
        if (_umaTextAsset == null)
        {
            _umaTextAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/_UMA Crowd/{gameObject.scene.name}/{gameObject.name}.txt");
        }
    }
#endif

    private IEnumerator Start()
    {
        _avatar.BuildCharacterEnabled = true;
        yield return null;
        _avatar.LoadFromRecipeString(_umaTextAsset.text);
    }
}
