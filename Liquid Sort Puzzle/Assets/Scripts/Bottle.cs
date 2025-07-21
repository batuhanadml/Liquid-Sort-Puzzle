using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BottleLiquidController))]
public class Bottle : MonoBehaviour
{
    [Header("Pour Settings")]
    [SerializeField] float rotationDuration = .5f;
    [SerializeField] float moveDuration = .5f;
    [SerializeField] AnimationCurve scaleCurve;

    [Header("Assign")]
    [SerializeField] GameObject selectionObject;
    [SerializeField] GameObject completedObject;
    BottleLiquidController liquidController;

    bool isCompleted = false;


    public void AddStartingLiquids(List<LiquidContainer> liquids)
    {
        if (liquidController == null)
            liquidController = GetComponent<BottleLiquidController>();

        liquidController.SetStartingLiquids(liquids);
    }

    public void ClearBottle()
    {
        isCompleted = false;
        selectionObject.SetActive(false);
        completedObject.SetActive(false);
    }


    #region Selection
    void OnMouseDown()
    {
        if (isCompleted) return;

        LiquidTransferManager.Instance.SelectBottle(this);
    }

    public void SelectBottle()
    {
        selectionObject.SetActive(true);
    }

    public void UnSelectBottle()
    {
        selectionObject.SetActive(false);
    }
    #endregion


    #region Transform Coroutines
    public IEnumerator RotationRoutine(float targetAngle)
    {
        float elabsed = 0f;
        float startAngle = transform.eulerAngles.z;

        if (startAngle > 180) startAngle -= 360;

        while (elabsed <= rotationDuration)
        {
            float ratio = elabsed / rotationDuration;
            float angleValue = Mathf.Lerp(startAngle, targetAngle, ratio);

            transform.eulerAngles = new(0, 0, angleValue);
            liquidController.UpdateScale(scaleCurve.Evaluate(angleValue));

            elabsed += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator MoveRoutine(Vector3 targetPos)
    {
        float elabsed = 0f;
        Vector3 startPos = transform.position;

        while (elabsed <= moveDuration)
        {
            float ratio = elabsed / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, ratio);

            elabsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion


    public void CheckCompleted()
    {
        if (isCompleted) return;

        if (isCompleted = liquidController.IsCompleted())
            completedObject.SetActive(true);
    }
    public bool IsCompleted() => isCompleted || Mathf.Approximately(GetAvailableFillAmount(), 1); //Empty or Completed

    public LiquidContainer GetTopContainer() => liquidController.GetTopContainer();
    public float GetAvailableFillAmount() => liquidController.GetAvailableFillAmount();
    
    public void AddLiquidToFirstContainer(LiquidData newData) => liquidController.AddLiquidToFirstContainer(newData);
    public IEnumerator ChangeLiquidRoutine(float changeValue) => liquidController.ChangeLiquidRoutine(changeValue);
}