using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeComponentsGenerator
{
    private ComputeShader shader;
    private RenderTexture components;
    private RenderTexture choppyX;
    private RenderTexture choppyZ;

    RenderTexture h0k;
    RenderTexture h0minusk;

    private int N;
    private int L;

    public TimeComponentsGenerator(RenderTexture h0k, RenderTexture h0minusk, int N, int L) {
        this.h0k = h0k;
        this.h0minusk = h0minusk;
        this.N = N;
        this.L = L;

        shader = Resources.Load<ComputeShader>("shaders/FourierComponents");

        components = new RenderTexture(N, N, 32, RenderTextureFormat.ARGBFloat);
        components.enableRandomWrite = true;
        components.Create();

        choppyX = new RenderTexture(N, N, 32, RenderTextureFormat.ARGBFloat);
        choppyX.enableRandomWrite = true;
        choppyX.Create();

        choppyZ = new RenderTexture(N, N, 32, RenderTextureFormat.ARGBFloat);
        choppyZ.enableRandomWrite = true;
        choppyZ.Create();
    }

    public void UpdateComponents(float time) {
        int kernelHandle = shader.FindKernel("CSMain");
        shader.SetFloat("time", time);
        shader.SetInt("L", L);
        shader.SetInt("N", N);

        shader.SetTexture(kernelHandle, "tilde_hkt_dy", components);
        shader.SetTexture(kernelHandle, "tilde_hkt_dx", choppyX);
        shader.SetTexture(kernelHandle, "tilde_hkt_dz", choppyZ);

        shader.SetTexture(kernelHandle, "h0k", h0k);
        shader.SetTexture(kernelHandle, "h0minusk", h0minusk);

        uint sizeX, sizeY, sizeZ;
        shader.GetKernelThreadGroupSizes(kernelHandle, out sizeX, out sizeY, out sizeZ);
        shader.Dispatch(kernelHandle, N / (int)sizeX, N / (int)sizeY, 1);
    }

    public RenderTexture GetComponents() {
        return components;
    }

    public RenderTexture GetChoppyX() {
        return choppyX;
    }

    public RenderTexture GetChoppyZ() {
        return choppyZ;
    }
}
