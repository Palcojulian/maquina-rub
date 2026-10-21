using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mantiene la pieza congelada (cinemática) hasta que la toca otra pieza que ya está en movimiento.
/// Así cada acción de la máquina ocurre solo como consecuencia de la anterior.
///
/// Un sensor (trigger) apenas más grande que la pieza detecta la llegada de la pieza activa y libera
/// esta justo antes del contacto, de modo que el choque real ocurre entre dos cuerpos dinámicos
/// y transmite la fuerza (contra un cuerpo cinemático la pieza que llega rebotaría como contra una pared).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ActivacionEnCadena : MonoBehaviour
{
    [Tooltip("Solo la primera pieza de la cadena (la esfera de INICIO) arranca activa.")]
    public bool activoAlInicio = false;
    [Tooltip("Cuánto sobresale el sensor de la pieza, en metros.")]
    public float margen = 0.03f;
    [Tooltip("Velocidad mínima de la pieza que llega para activar esta.")]
    public float velocidadMinima = 0.03f;

    public bool Activo { get; private set; }

    Rigidbody rb;
    CollisionDetectionMode modoOriginal;
    readonly List<GameObject> sensores = new List<GameObject>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        modoOriginal = rb.collisionDetectionMode;
        if (activoAlInicio) { Activo = true; return; }

        // Cinemático no admite continua dinámica: se usa especulativa mientras espera
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        CrearSensores();
    }

    void CrearSensores()
    {
        foreach (var col in GetComponentsInChildren<Collider>())
        {
            if (col.isTrigger || col.attachedRigidbody != rb) continue;
            var s = lossyAbs(col.transform.lossyScale);
            var go = new GameObject("Sensor_" + col.name);
            go.transform.SetParent(col.transform, false);
            go.layer = col.gameObject.layer;
            switch (col)
            {
                case BoxCollider b:
                    var bs = go.AddComponent<BoxCollider>();
                    bs.center = b.center;
                    bs.size = b.size + new Vector3(2 * margen / s.x, 2 * margen / s.y, 2 * margen / s.z);
                    bs.isTrigger = true;
                    break;
                case SphereCollider sp:
                    var ss = go.AddComponent<SphereCollider>();
                    ss.center = sp.center;
                    ss.radius = sp.radius + margen / Mathf.Max(s.x, s.y, s.z);
                    ss.isTrigger = true;
                    break;
                case CapsuleCollider c:
                    var cs = go.AddComponent<CapsuleCollider>();
                    cs.center = c.center; cs.direction = c.direction;
                    cs.radius = c.radius + margen / Mathf.Max(s.x, s.y, s.z);
                    cs.height = c.height + 2 * margen / Mathf.Max(s.x, s.y, s.z);
                    cs.isTrigger = true;
                    break;
                default:
                    Destroy(go);
                    continue;
            }
            sensores.Add(go);
        }
    }

    static Vector3 lossyAbs(Vector3 v) =>
        new Vector3(Mathf.Max(Mathf.Abs(v.x), 1e-4f), Mathf.Max(Mathf.Abs(v.y), 1e-4f), Mathf.Max(Mathf.Abs(v.z), 1e-4f));

    void OnTriggerStay(Collider otro)
    {
        if (Activo || otro.isTrigger) return;
        var orb = otro.attachedRigidbody;
        if (orb == null || orb == rb || orb.isKinematic) return;
        // solo cuenta una pieza que ya está en marcha (no basta con estar apoyada al lado)
        if (orb.linearVelocity.magnitude < velocidadMinima && orb.angularVelocity.magnitude < velocidadMinima * 10f) return;
        Activar();
    }

    public void Activar()
    {
        if (Activo) return;
        Activo = true;
        foreach (var s in sensores) if (s != null) Destroy(s);
        sensores.Clear();
        rb.isKinematic = false;
        rb.collisionDetectionMode = modoOriginal;
        rb.WakeUp();
        Debug.Log($"[Cadena] {Time.timeSinceLevelLoad:F2}s activada {name}");
    }
}
