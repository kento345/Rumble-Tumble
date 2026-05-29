using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerMeter1 : MonoBehaviour
{
    [SerializeField] private Image MeterImage;

    private float meterSpeed = 1.0f;
    private Coroutine meter;

    [SerializeField] private float MaxChargeTime = 1.5f;

    private PlayerController1 pc1;


    private void Start()
    {
        pc1 = GetComponent<PlayerController1>();
        MeterImage.fillAmount = 0;
    }

    private void Update()
    {
        float speed = 1f / MaxChargeTime;
        if(pc1.isStrt)
        {
            MeterImage.fillAmount += speed * Time.deltaTime;
        }
        else if(!pc1.isStrt) 
        {
            MeterImage.fillAmount = 0;
        }

        // 0〜1 の範囲に制限
        MeterImage.fillAmount = Mathf.Clamp01(MeterImage.fillAmount);

        // Player のタックル力 (t) に反映
        pc1.SetCharge(MeterImage.fillAmount * pc1.chargeMax);
    }
}
