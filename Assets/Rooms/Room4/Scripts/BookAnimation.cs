using UnityEngine;

public class BookAnimation : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;

    [SerializeField]
    private Texture2D background;

    private void OnEnable()
    {
        const string DEFAULT_CLIP = "Closed";
        const int PAGE_1_MATERIAL_INDEX = 1;
        const int PAGE_2_MATERIAL_INDEX = 2;
        const string TEXTURE_NAME = "_AlbedoMap";
        const string BOOL_NAME = "Closed";

        animator.Play(DEFAULT_CLIP);

        Material page1Material = meshRenderer.materials[PAGE_1_MATERIAL_INDEX];
        Material page2Material = meshRenderer.materials[PAGE_2_MATERIAL_INDEX];

        page1Material.SetTexture(TEXTURE_NAME, background);
        page2Material.SetTexture(TEXTURE_NAME, background);

        animator.SetBool(BOOL_NAME, false);
    }
}
