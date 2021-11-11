using UnityEngine;

public class TorchBehaviour : MonoBehaviour
{
    GameManagerBehaviour    gameManager;
    int                     torchID;
    Light                   light;

    public void Init(GameManagerBehaviour gm, int id)
    {
        gameManager = gm;
        torchID = id;

        light = GetComponentInChildren<Light>();
        light.intensity = 1;
    }

    // TorchUpdate Intentionally Empty
    public void TorchUpdate()
    {

    }

    public float GetTorchLightIntensity()
    {
        return light.intensity;
    }

    public void LightUpTorch()
    {
        light.intensity = 1;
    }

    public void LightDownTorch()
    {
        light.intensity = 0;
    }
}
