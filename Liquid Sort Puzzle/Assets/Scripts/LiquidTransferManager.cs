using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidTransferManager : MonoBehaviour
{
    #region Singleton
    public static LiquidTransferManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    #endregion

    public static Action OnLiquidTransferCompleted;

    [Header("Settings")]
    [SerializeField] Vector3 pourPositionOffset = new(-1f, 1.5f, 0);

    Bottle selectedBottle;
    bool isBusy = false;

    void Update()
    {
        if (selectedBottle != null && !isBusy && 
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
            ClearSelection();
    }

    public void SelectBottle(Bottle bottle)
    {
        if (isBusy) return;

        if (selectedBottle == null)
        {
            if (bottle.GetAvailableFillAmount() < 1)
            {
                selectedBottle = bottle;
                selectedBottle.SelectBottle();
            }
        }
        else if (selectedBottle == bottle)
        {
            ClearSelection();
        }
        else TransferLiquid(bottle);
    }

    void TransferLiquid(Bottle targetBottle)
    {
        LiquidContainer fromContainer = selectedBottle.GetTopContainer();
        LiquidContainer toContainer = targetBottle.GetTopContainer();

        // Return if target bottle has different color at the top
        if (toContainer.data != null && toContainer.data != fromContainer.data)
            return;

        // Return if target bottle is full
        float availableFill = targetBottle.GetAvailableFillAmount();
        if (availableFill == 0) return;

        // Add color data to first container of target (empty) bottle before fill
        if (availableFill == 1)
            targetBottle.AddLiquidToFirstContainer(fromContainer.data);

        float transferAmount = Mathf.Min(availableFill, fromContainer.amount);

        StartCoroutine(PourRoutine(targetBottle, transferAmount));
    }

    IEnumerator PourRoutine(Bottle targetBottle, float transferAmount)
    {
        isBusy = true;
        Vector3 originalPos = selectedBottle.transform.position;

        // Move and rotate
        yield return selectedBottle.MoveRoutine(targetBottle.transform.position + pourPositionOffset);
        yield return selectedBottle.RotationRoutine(-90);

        // Transfer
        StartCoroutine(targetBottle.ChangeLiquidRoutine(transferAmount));
        yield return selectedBottle.ChangeLiquidRoutine(-transferAmount);

        // Rotate and move back
        yield return selectedBottle.RotationRoutine(0);
        yield return selectedBottle.MoveRoutine(originalPos);

        targetBottle.CheckCompleted();
        OnLiquidTransferCompleted?.Invoke();

        ClearSelection();
        isBusy = false;
    }

    void ClearSelection()
    {
        if (selectedBottle == null) return;

        selectedBottle.UnSelectBottle();
        selectedBottle = null;
    }
}
