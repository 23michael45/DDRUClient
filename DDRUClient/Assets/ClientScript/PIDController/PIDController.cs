using UnityEngine;
using System.Collections;

namespace PIDController
{
    public class PIDController : MonoBehaviour
    {
        float CTE_old = 0f;
        float CTE_sum = 0f;

        //PID parameters
        public float tau_P = 0f;
        public float tau_I = 0f;
        public float tau_D = 0f;


        public float GetSteerFactorFromPIDController(float CTE)
        {
            float alpha = tau_P * CTE;
            return alpha;
        }

        public float GetSteerFactorFromPIDController_(float CTE)
        {
            //The steering factor
            float alpha = 0f;


            //P
            alpha = tau_P * CTE;


            //I
            CTE_sum += Time.fixedDeltaTime * CTE;

            //Sometimes better to just sum the last errors
            float averageAmount = 20f;

            CTE_sum = CTE_sum + ((CTE - CTE_sum) / averageAmount);

            alpha += tau_I * CTE_sum;


            //D
            float d_dt_CTE = (CTE - CTE_old) / Time.fixedDeltaTime;

            alpha += tau_D * d_dt_CTE;

            CTE_old = CTE;


            if (System.Single.IsNaN(alpha))
            {
                alpha = 0;
            }

            return alpha;
        }
    }
}