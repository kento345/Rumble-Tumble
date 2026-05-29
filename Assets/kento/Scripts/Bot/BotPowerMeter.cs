using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BotPowerMeter : MonoBehaviour
{
    [SerializeField] private Image MeterImage;

    private float meterSpeed = 1.0f;
    private Coroutine meter;

    [SerializeField] private float MaxChargeTime = 1.5f;

    private BotPlayerController1 bc;


    private void Start()
    {
        bc = GetComponent<BotPlayerController1>();
        MeterImage.fillAmount = 0;
    }

    private void Update()
    {
        float speed = 1f / MaxChargeTime;
        if (bc.isStrt)
        {
            MeterImage.fillAmount += speed * Time.deltaTime;
        }
        else if (!bc.isStrt)
        {
            MeterImage.fillAmount = 0;
        }

        // 0〜1 の範囲に制限
        MeterImage.fillAmount = Mathf.Clamp01(MeterImage.fillAmount);

        // Player のタックル力 (t) に反映
        bc.SetCharge(MeterImage.fillAmount * bc.chargeMax);
    }
}
