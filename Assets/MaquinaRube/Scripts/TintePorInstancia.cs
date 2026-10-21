using UnityEngine;

/// <summary>
/// Aplica un color por instancia sin duplicar el material compartido
/// (MaterialPropertyBlock sobre _BaseColor de URP/Lit).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TintePorInstancia : MonoBehaviour
{
    public Color color = Color.white;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static MaterialPropertyBlock bloque;

    void OnEnable() => Aplicar();
    void OnValidate() => Aplicar();

    public void Aplicar()
    {
        var r = GetComponent<Renderer>();
        if (r == null) return;
        bloque ??= new MaterialPropertyBlock();
        r.GetPropertyBlock(bloque);
        bloque.SetColor(BaseColorId, color);
        r.SetPropertyBlock(bloque);
    }
}
