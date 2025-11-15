using System.Collections.Generic;
using UnityEngine;

public class AnimatorBridgeSafe : MonoBehaviour
{
    [Tooltip("Arrastra aquí el componente Animator del GameObject")]
    public Animator animator;

    [Tooltip("Nombre exacto del parámetro float que controla la velocidad")]
    public string speedParam = "Speed";

    [Tooltip("Nombre exacto del trigger de muerte u otra acción")]
    public string dieTrigger = "Die";

    HashSet<string> paramCache = new HashSet<string>();
    bool warnedNoController;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        RefreshParameterCache();
    }

    // Llamar si asignas el Controller en tiempo de ejecución desde el Inspector
    public void RefreshParameterCache()
    {
        paramCache.Clear();
        if (animator == null)
        {
            Debug.LogWarning("AnimatorBridgeSafe: no hay Animator asignado.");
            return;
        }

        // Evitar acceder a animator.parameters si no hay controller asignado
        if (animator.runtimeAnimatorController == null)
        {
            if (!warnedNoController)
            {
                Debug.LogWarning("AnimatorBridgeSafe: el Animator no tiene un AnimatorController asignado. Asigna el Controller en el Inspector para usar parámetros.");
                warnedNoController = true;
            }
            return;
        }

        foreach (var p in animator.parameters)
            paramCache.Add(p.name);
    }

    public void SetSpeed(float value)
    {
        if (animator == null) return;
        if (animator.runtimeAnimatorController == null) return;
        if (!paramCache.Contains(speedParam)) return;
        animator.SetFloat(speedParam, value);
    }

    public void TriggerDie()
    {
        if (animator == null) return;
        if (animator.runtimeAnimatorController == null) return;
        if (!paramCache.Contains(dieTrigger)) return;
        animator.SetTrigger(dieTrigger);
    }

    public bool HasParameter(string name)
    {
        return paramCache.Contains(name);
    }
}
