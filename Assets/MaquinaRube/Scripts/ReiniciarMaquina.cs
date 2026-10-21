using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Ajusta la física de la máquina al entrar en Play y permite reiniciarla con la tecla R.
/// </summary>
public class ReiniciarMaquina : MonoBehaviour
{
    [Tooltip("Paso de física: más fino evita que las esferas atraviesen piezas delgadas.")]
    public float pasoFisica = 0.005f;
    public int iteracionesSolver = 12;

    void Awake()
    {
        Time.fixedDeltaTime = pasoFisica;
        Physics.defaultSolverIterations = iteracionesSolver;
        Physics.defaultSolverVelocityIterations = 4;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        bool reiniciar = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        bool reiniciar = Input.GetKeyDown(KeyCode.R);
#endif
        if (!reiniciar) return;
        var escena = SceneManager.GetActiveScene();
#if UNITY_EDITOR
        // En el editor la escena no necesita estar en Build Settings
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(escena.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(escena.buildIndex);
#endif
    }
}
