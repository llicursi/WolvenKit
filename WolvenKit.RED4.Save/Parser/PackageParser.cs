using WolvenKit.RED4.Archive.Buffer;
using WolvenKit.RED4.Archive.IO;
using WolvenKit.RED4.Save.IO;
using WolvenKit.RED4.Types;

namespace WolvenKit.RED4.Save;

public class PackageParser : INodeParser
{
    public virtual void Read(BinaryReader reader, NodeEntry node) => Read(reader, node, typeof(inkWidgetLibraryResource));

    protected void Read(BinaryReader reader, NodeEntry node, Type dummyType)
    {
        var dummyBuffer = new RedBuffer();

        var subReader = new PackageReader(reader);
        subReader.ReadBuffer(dummyBuffer, dummyType);

        node.Value = dummyBuffer.Data;
    }

    public virtual void Write(NodeWriter writer, NodeEntry node) => Write(writer, node, typeof(inkWidgetLibraryResource));
    public virtual void Write(NodeWriter writer, NodeEntry node, Type dummyType)
    {
        using var ms = new MemoryStream();
        using var subWriter = new PackageWriter(ms);
        subWriter.WritePackage((Package04)node.Value, dummyType);

        writer.Write(ms.ToArray());
    }
}
