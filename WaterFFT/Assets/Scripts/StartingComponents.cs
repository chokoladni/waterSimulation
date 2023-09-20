using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingComponents
{
    private ComputeShader shader;
    private RenderTexture h0k;
    private RenderTexture h0minusk;
    private int kernelHandle;
    private int N;

    public StartingComponents(int N) {
        this.N = N;

        shader = Resources.Load<ComputeShader>("shaders/StartingFourierComponents");
        kernelHandle = shader.FindKernel("CSMain");

        h0k = new RenderTexture(N, N, 32, RenderTextureFormat.ARGBFloat);
        h0k.enableRandomWrite = true;
        h0k.Create();

        h0minusk = new RenderTexture(N, N, 32, RenderTextureFormat.ARGBFloat);
        h0minusk.enableRandomWrite = true;
        h0minusk.Create();
    }

    public void GenerateComponents(int L, float A, float windSpeed, Vector2 windDirection) {
        shader.SetInt("N", N);
        shader.SetInt("L", L);
        shader.SetFloat("A", A);
        shader.SetVector("windDirection", windDirection.normalized);
        shader.SetFloat("windSpeed", windSpeed);
        
        var noise0 = RandomNoiseTexture.GenerateTexture(N);
        var noise1 = RandomNoiseTexture.GenerateTexture(N);
        var noise2 = RandomNoiseTexture.GenerateTexture(N);
        var noise3 = RandomNoiseTexture.GenerateTexture(N);

        shader.SetTexture(kernelHandle, "noise0", noise0);
        shader.SetTexture(kernelHandle, "noise1", noise1);
        shader.SetTexture(kernelHandle, "noise2", noise2);
        shader.SetTexture(kernelHandle, "noise3", noise3);

        shader.SetTexture(kernelHandle, "h0kTexOut", h0k);
        shader.SetTexture(kernelHandle, "h0minuskTexOut", h0minusk);

        uint sizeX, sizeY, sizeZ;
        shader.GetKernelThreadGroupSizes(kernelHandle, out sizeX, out sizeY, out sizeZ);
        shader.Dispatch(kernelHandle, N / (int)sizeX, N / (int)sizeY, 1);
    }

    public RenderTexture GetH0k() {
        return h0k;
    }

    public RenderTexture GetH0minusk() {
        return h0minusk;
    }
}
