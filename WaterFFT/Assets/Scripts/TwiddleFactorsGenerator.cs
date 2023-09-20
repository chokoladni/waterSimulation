using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwiddleFactorsGenerator
{

    private ComputeShader shader;

    public TwiddleFactorsGenerator() {
        shader = Resources.Load<ComputeShader>("shaders/TwiddleFactors");
    }
    

    public RenderTexture Generate(int N) {
        int log2N = Mathf.CeilToInt(Mathf.Log(N, 2));
        RenderTexture twiddleFactors = new RenderTexture(log2N, N, 32, RenderTextureFormat.ARGBFloat);
        twiddleFactors.enableRandomWrite = true;
        twiddleFactors.Create();

        ComputeBuffer buffer = new ComputeBuffer(N, sizeof(int));
        setupBufferData(buffer, (uint)N);

        int kernelHandle = shader.FindKernel("CSMain");
        shader.SetInt("N", N);
        shader.SetBuffer(kernelHandle, "bit_reversed", buffer);
        shader.SetTexture(kernelHandle, "twiddleFactorsTex", twiddleFactors);

        uint sizeX, sizeY, sizeZ;
        shader.GetKernelThreadGroupSizes(kernelHandle, out sizeX, out sizeY, out sizeZ);
       
        shader.Dispatch(kernelHandle, N / (int)sizeX, N / (int)sizeY, 1);
        //TODO: test shader.Dispatch(kernelHandle, log2N / (int)sizeX, N / (int)sizeY, 1);

        buffer.Release();

        return twiddleFactors;
    }

    private void setupBufferData(ComputeBuffer buffer, uint size) {
        //TODO: test uint size = buffer.count;
        int bits = (int)Mathf.Log(size, 2);
        int[] data = new int[size];
        for(int i = 0; i < size; i++) {
            string binaryString = Convert.ToString(i, 2).PadLeft(bits, '0');
            int reversed = Convert.ToInt32(Reverse(binaryString), 2);
            data[i] = reversed;
        }

        buffer.SetCounterValue(size);
        buffer.SetData(data);
    }

    private string Reverse(string s) {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
}
