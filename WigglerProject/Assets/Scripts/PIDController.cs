using UnityEngine;

public class PIDController
{
    public enum DerivativeMeasurement
    {
        Velocity,
        ErrorRateOfChange
    }
    public float proportionalGain;
    public float integralGain;
    public float derivativeGain;

    public float errorLast;
    public float valueLast;

    public float integrationStored;
    
    //set to input limits of system as a good starting value
    public float integralSaturation;
    
    //set to input values you want to accept
    public float outputMin = -1;
    public float outputMax = 1;
    
    public DerivativeMeasurement derivativeMeasurement;

    public bool derivativeInitialised;

   
    public float Update(float deltaTime, float currentValue, float targetValue)
    {
        float error = targetValue - currentValue;
        //distance is the absolute value of error
        
        //pTerm is a spring
        float pTerm = proportionalGain * error ;
        
        //calculate I term
        integrationStored = Mathf.Clamp(integrationStored +(error * deltaTime), -integralSaturation, integralSaturation);
        
        float iTerm = integralGain * integrationStored;
        
        
        //calculate dTerms
        float errorRateOfChange = (error - errorLast) / deltaTime;
        float valueRateOfChange = (currentValue - valueLast) / deltaTime;
        errorLast = error;
        valueLast = currentValue;
        
        
        //pick which d term to use
        float deriveMeasure = 0;
        

        if (derivativeInitialised)
        {
            switch (derivativeMeasurement)
            {
                case DerivativeMeasurement.Velocity:
                    deriveMeasure = -valueRateOfChange;
                    break;
                default:
                    deriveMeasure = errorRateOfChange;
                    break;
            }
        }

        else
        {
            derivativeInitialised = true;
        }


        //dTerm acts as a dampener
        float dTerm = derivativeGain * deriveMeasure;
        
        float result = pTerm + iTerm + dTerm;
        return Mathf.Clamp(result, outputMin,  outputMax);
    }
    
    

    public float UpdateAngle(float deltaTime, float currentAngle, float targetAngle)
    {
        float error = AngleDifference(targetAngle, currentAngle);

        //calculate P term
        float pTerm = proportionalGain * error;

        //calculate I term
        integrationStored = Mathf.Clamp(integrationStored + (error * deltaTime), -integralSaturation, integralSaturation);
        float iTerm = integralGain * integrationStored;

        //calculate both D terms
        float errorRateOfChange = AngleDifference(error, errorLast) / deltaTime;
        errorLast = error;

        float valueRateOfChange = AngleDifference(currentAngle, valueLast) / deltaTime;
        valueLast = currentAngle;

        //choose D term to use
        float deriveMeasure = 0;

        if (derivativeInitialised)
        {
            switch (derivativeMeasurement)
            {
                case DerivativeMeasurement.Velocity:
                    deriveMeasure = -valueRateOfChange;
                    break;
                default:
                    deriveMeasure = errorRateOfChange;
                    break;
            }
        } 
        else 
        {
            derivativeInitialised = true;
        }

        float dTerm = derivativeGain * deriveMeasure;

        float result = pTerm + iTerm + dTerm;

        return Mathf.Clamp(result, outputMin, outputMax);
    }

    public void ResetDerivative()
    {
        derivativeInitialised = false; 
    }

    float AngleDifference(float a, float b)
    {
        //limits angle range between -180 and 180
        return (a - b + 540) % 360 - 180;
    }
}