using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleLiquidController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float fillSpeed = .6f;
    public List<LiquidContainer> liquids = new(4); //Debug
    float fill;

    [Header("Components")]
    [SerializeField] MeshRenderer rend;
    Material mat;

    readonly static string[] colorKeys = new string[] { "_Color1", "_Color2", "_Color3", "_Color4" };
    readonly static string[] fresnelKeys = new string[] { "_FresnelColor1", "_FresnelColor2", "_FresnelColor3", "_FresnelColor4" };
    readonly static string[] stepKeys = new string[] { "_LiquidStep1", "_LiquidStep2", "_LiquidStep3", "_LiquidStep4" };
    readonly static string surfaceColorKey = "_SurfaceColor";
    readonly static string fillKey = "_Fill";
    readonly static string scaleMultKey = "_ScaleMult";

    public void SetStartingLiquids(List<LiquidContainer> _liquids)
    {
        liquids = new(_liquids);
        mat = rend.material;

        float step = 0;

        for (int i = 0; i < liquids.Count; i++)
        {
            LiquidData data = liquids[i].data;
            if (data == null) continue;

            mat.SetColor(colorKeys[i], data.LiquidColor);
            mat.SetColor(fresnelKeys[i], data.FresnelColor);

            step += liquids[i].amount;
            mat.SetFloat(stepKeys[i], step);
        }

        fill = step;
        mat.SetFloat(fillKey, fill);
        UpdateSurfaceColor();
    }


    #region Liquid Changes
    public void AddLiquidToFirstContainer(LiquidData newData)
    {
        liquids[0].data = newData;
        mat.SetColor(colorKeys[0], newData.LiquidColor);
        mat.SetColor(fresnelKeys[0], newData.FresnelColor);
        mat.SetColor(surfaceColorKey, newData.SurfaceColor);
    }

    public IEnumerator ChangeLiquidRoutine(float changeValue)
    {
        int containerIndex = GetTopContainerIndex();

        float targetFill = Mathf.Clamp(fill + changeValue, 0, 1);
        float step = mat.GetFloat(stepKeys[containerIndex]);
        float targetStep = step + changeValue;

        while (!Mathf.Approximately(fill, targetFill))
        {
            fill = Mathf.MoveTowards(fill, targetFill, fillSpeed * Time.deltaTime);
            mat.SetFloat(fillKey, fill);

            step = Mathf.MoveTowards(step, targetStep, fillSpeed * Time.deltaTime);
            mat.SetFloat(stepKeys[containerIndex], step);

            yield return null;
        }

        // Set final values
        fill = targetFill;
        mat.SetFloat(fillKey, fill);
        mat.SetFloat(stepKeys[containerIndex], targetStep);

        // Update container amount
        liquids[containerIndex].amount += changeValue;

        // Clear top container if necessary
        if (liquids[containerIndex].amount < .001f)
        {
            liquids[containerIndex].Clear();

            mat.SetFloat(stepKeys[containerIndex], 1);
            UpdateSurfaceColor();
        }
    }

    void UpdateSurfaceColor()
    {
        LiquidContainer container = GetTopContainer();
        if (container.data == null) return;

        mat.SetColor(surfaceColorKey, container.data.SurfaceColor);
    }
    #endregion


    public void UpdateScale(float value)
    {
        mat.SetFloat(scaleMultKey, (fill == 0 ? 1 : value));
    }

    public bool IsCompleted()
    {
        return liquids[0].amount >= .99f;
    }

    public LiquidContainer GetTopContainer() => liquids[GetTopContainerIndex()];

    public float GetAvailableFillAmount() => 1 - fill;

    int GetTopContainerIndex()
    {
        for (int i = liquids.Count - 1; i >= 0; i--)
        {
            if (liquids[i].amount > 0)
                return i;
        }
        return 0; //Fallback to first container
    }
}


[System.Serializable]
public class LiquidContainer
{
    public LiquidData data;
    public float amount;

    public LiquidContainer(LiquidData data, float amount = 1f)
    {
        this.data = data;
        this.amount = amount;
    }

    public void Clear()
    {
        data = null;
        amount = 0;
    }
}
