using UnityEngine;

[ExecuteAlways]
public class WindzoneToShader : MonoBehaviour
{
    [SerializeField]
    private WindZone windZone;

    private void OnValidate()
    {
        FindWindZone();
    }

    private void Start()
    {
        FindWindZone();
    }

    void Update()
    {
        ApplySettings();
    }

    void ApplySettings()
    {
        if (windZone != null)
        {
            Shader.SetGlobalVector("_WINDZONE_Direction",
                                   windZone.transform.forward);

            // wind speed/strength (e.g. 1)
            Shader.SetGlobalFloat("_WINDZONE_Main",
                                  windZone.windMain);

            // How many pulses of wind per second
            Shader.SetGlobalFloat("_WINDZONE_Pulse_Frequency",
                                  windZone.windPulseFrequency);

            // Pulse strength, e.g. 0.5
            Shader.SetGlobalFloat("_WINDZONE_Pulse_Magnitude",
                                  windZone.windPulseMagnitude);

            // Degree of variation (e.g. 1 is lots of variation)
            Shader.SetGlobalFloat("_WINDZONE_Turbulence",
                                  windZone.windTurbulence);
        }
    }

    private void FindWindZone()
    {
        if (windZone == null)
        {
            windZone = gameObject.GetComponent<WindZone>();
        }
    }
}