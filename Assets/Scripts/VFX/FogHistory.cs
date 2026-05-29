using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public sealed class FogHistory : CameraHistoryItem
{
    private int m_Id;
    private static readonly string m_Name = "_FogTexture";
    
    private RenderTextureDescriptor m_Descriptor;
    private Hash128 m_DescKey;

    /// <inheritdoc />
    public override void OnCreate(BufferedRTHandleSystem owner, uint typeId)
    {
        base.OnCreate(owner, typeId);
        m_Id = MakeId(0);
    }

    /// <summary>
    /// Get the current history texture.
    /// Current history might not be valid yet. It is valid only after executing the producing render pass.
    /// </summary>
    /// <param name="eyeIndex">Eye index, typically XRPass.multipassId.</param>
    /// <returns>The texture.</returns>
    public RTHandle GetCurrentTexture()
    {
        return GetCurrentFrameRT(m_Id);
    }

    /// <summary>
    /// Get the previous history texture.
    /// Previous history might not be valid yet. It is valid only after executing the producing render pass.
    /// </summary>
    /// <param name="eyeIndex">Eye index, typically XRPass.multipassId.</param>
    /// <returns>The texture.</returns>
    public RTHandle GetPreviousTexture()
    {
        return GetPreviousFrameRT(m_Id);
    }

    private bool IsAllocated()
    {
        return GetCurrentTexture() != null;
    }

    // True if the desc changed, graphicsFormat etc.
    private bool IsDirty(ref RenderTextureDescriptor desc)
    {
        return m_DescKey != Hash128.Compute(ref desc);
    }

    private void Alloc(ref RenderTextureDescriptor desc)
    {
        // In generic case, the current texture might not have been written yet. We need double buffering.
        AllocHistoryFrameRT(m_Id, 2, ref desc, m_Name);

        m_Descriptor = desc;
        m_DescKey = Hash128.Compute(ref desc);
    }

    /// <summary>
    /// Release the history texture(s).
    /// </summary>
    public override void Reset()
    {
        ReleaseHistoryFrameRT(m_Id);
    }

    internal RenderTextureDescriptor GetHistoryDescriptor(ref RenderTextureDescriptor cameraDesc)
    {
        var colorDesc = cameraDesc;
        colorDesc.depthStencilFormat = GraphicsFormat.None;
        colorDesc.mipCount = 0;
        colorDesc.msaaSamples = 1;

        return colorDesc;
    }

    // Return true if the RTHandles were reallocated.
    internal void Update(ref RenderTextureDescriptor cameraDesc)
    {
        if (cameraDesc.width > 0 && cameraDesc.height > 0 && cameraDesc.graphicsFormat != GraphicsFormat.None)
        {
            var historyDesc = GetHistoryDescriptor(ref cameraDesc);

            if (IsDirty(ref historyDesc))
                Reset();

            if (!IsAllocated())
            {
                Alloc(ref historyDesc);
            }
        }
    }
}
