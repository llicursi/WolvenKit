using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using SharpGLTF.Validation;
using WolvenKit.RED4.Archive;
using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.Common.Model.Arguments
{
    /// <summary>
    /// Import Arguments
    /// </summary>
    public abstract class ImportArgs : ImportExportArgs
    {
        [Category("General Import Settings")]
        [Display(Name = "Use existing file", Order = 0)]
        [Description("If checked the corresponding archive file will be used for importing.")]
        public bool Keep { get; set; } = true;
    }


    /// <summary>
    /// Common Import Arguments
    /// </summary>
    public class CommonImportArgs : ImportArgs
    {
    }

    /// <summary>
    /// Fnt Import Arguments
    /// </summary>
    public class OpusImportArgs : ImportArgs
    {
        public bool UseMod { get; set; }

        public override string ToString() => ".WAV";
    }

    /// <summary>
    /// Fnt Import Arguments
    /// </summary>
    public class FntImportArgs : ImportArgs
    {
        public override string ToString() => ".FNT";
    }

    /// <summary>
    /// XBM Import Arguments
    /// </summary>
    public class XbmImportArgs : ImportArgs
    {
        [Category("General Import Settings")]
        [Description("Select the texture group of the imported item")]
        public GpuWrapApieTextureGroup TextureGroup { get; set; } = GpuWrapApieTextureGroup.TEXG_Generic_Color;

        [Category("General Import Settings")]
        [Description("If true, the file will be handled as a SRGB file")]
        public bool IsGamma { get; set; } = false;

        [Category("General Import Settings")]
        [Description("Vertical Flip")]
        public bool VFlip { get; set; } = true;



        [Category("Image Import Settings")]
        public ETextureRawFormat RawFormat { get; set; } = ETextureRawFormat.TRF_TrueColor;

        [Category("Image Import Settings")]
        public ETextureCompression Compression { get; set; }

        [Category("Image Import Settings")]
        [Description("If true, mipMaps will be generated")]
        public bool GenerateMipMaps { get; set; } = true;

        [Category("Image Import Settings")]
        public bool IsStreamable { get; set; } = true;



        [Category("Image Import Settings")]
        [Description("PremultiplyAlpha")]
        public bool PremultiplyAlpha { get; set; } = true;

        public XbmImportArgs()
        {
            Keep = false;
            GenerateMipMaps = true;
        }

        /// <summary>
        /// String Override to display info in datagrid.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => TextureGroup.ToString();
    }

    /// <summary>
    /// Mesh Import Arguments
    /// </summary>
    public class GltfImportArgs : ImportArgs
    {
        /// <summary>
        /// Should a Material.Json be imported?
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "Import with Material.Json")]
        [Description("If selected materials will be updated from a Material.json file.")]
        public bool ImportMaterials { get; set; } = false;

        /// <summary>
        /// Should only materials be imported and no mesh data be changed?
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "Import Material.Json Only")]
        [Description("If selected only materials will be updated from a Material.json file. Mesh geometry will remain unchanged.")]
        public bool ImportMaterialOnly { get; set; } = false;

        /// <summary>
        /// Validation type for the selected GLB/GLTF.
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "GLTF Validation Checks")]
        [Description("Optional validation check for glb/glTF files")]
        public ValidationMode ValidationMode { get; set; } = ValidationMode.Skip;

        /// <summary>
        /// RedEngine4 Cooked File type for the selected GLB/GLTF.
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "Target File Format")]
        public GltfImportAsFormat ImportFormat { get; set; } = GltfImportAsFormat.Mesh;


        /// <summary>
        /// Fills empty sub meshes with dummy data
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "Preserve Submesh Order (experimental)")]
        [Description("If selected empty submesh slots will be filled with placeholder data. This preserves the original submesh-material index.")]
        public bool FillEmpty { get; set; } = false;

        /// <summary>
        /// Selected Rig for Mesh WithRig Export. ALWAYS USE THE FIRST ENTRY IN THE LIST.
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "Select base mesh (experimental)")]
        [Description("Select a base mesh to import on.")]
        public List<FileEntry> BaseMesh { get; set; }

        /// <summary>
        /// Uses a selected mesh from archives as base mesh for import instead of mod project archive directory mesh
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "Use selected base mesh (experimental)")]
        [Description("If checked the specified mesh file will be used for importing.")]
        public bool SelectBase { get; set; } = false;

        /// <summary>
        /// Assigns found LOD0 submeshes to LOD8 to allow import of certain meshes that handle LOD0 as LOD8.
        /// </summary>
        [Category("Import Settings")]
        [Display(Name = "Contains LOD8 named LOD0")]
        [Description("If checked the included LOD0 submesh will be handled as LOD8")]
        public bool ReplaceLod { get; set; } = false;

        /// <summary>
        /// Selected Rig for Mesh WithRig Export. ALWAYS USE THE FIRST ENTRY IN THE LIST.
        /// </summary>
        [Category("WithRig Settings")]
        [Display(Name = "Select rig (experimental)")]
        [Description("Select a rig to import within the mesh.")]
        public List<FileEntry> Rig { get; set; }


        /// <summary>
        /// UNKNOWN
        /// </summary>
        [Category("WithRig Settings")]
        [Display(Name = "Use selected rig (experimental)")]
        [Description("If selected the corresponding archive file will be used for importing.")]
        public bool KeepRig { get; set; } = false;

        /// <summary>
        /// List of Archives for Morphtarget Import.
        /// </summary>
        [Browsable(false)]
        public List<ICyberGameArchive> Archives { get; set; } = new();
        /// <summary>
        /// String Override to display info in datagrid.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"Mesh/Morphtarget | Import Format :  {ImportFormat}";
    }

    public enum GltfImportAsFormat
    {
        Mesh,
        Morphtarget,
        Anims,
        MeshWithRig,
        Rig
    }

    public class MlmaskImportArgs : ImportArgs
    {
        /// <summary>
        /// String Override to display info in datagrid.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "MLMASK";
    }

    /// <summary>
    /// Re to Animset import arguments
    /// </summary>
    public class ReImportArgs : ImportArgs
    {
        [Browsable(false)]
        public string RedMod { get; set; } = "";
        [Browsable(false)]
        public string Depot { get; set; } = "";
        [Browsable(false)]
        public string Input { get; set; } = "";
        [Browsable(false)]
        public string Animset { get; set; } = "";
        [Browsable(false)]
        public string AnimationToRename { get; set; } = "";
        [Category("Import Settings")]
        [Display(Name = "Outfile")]
        [Description("resource .animset file name to write (resource path must start with base//)")]
        public string Output { get; set; } = "";
        /// <summary>
        /// String Override to display info in datagrid.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"{Path.GetFileName(Animset)} - {AnimationToRename}";
    }

    // Formats used in the v1.52 game files
    //public enum SupportedCompressionFormats
    //{
    //    TCM_None, // = 0,
    //    TCM_DXTNoAlpha, // = 1,
    //    TCM_DXTAlpha, // = 2,
    //    // TCM_RGBE, // = 3,
    //    TCM_Normalmap, // = 4,
    //    TCM_Normals_DEPRECATED, // = 5,
    //    TCM_NormalsHigh_DEPRECATED, // = 6,
    //    // TCM_NormalsGloss_DEPRECATED, // = 7,
    //    // TCM_TileMap, // = 8,
    //    TCM_DXTAlphaLinear, // = 9,
    //    TCM_QualityR, // = 10,
    //    TCM_QualityRG, // = 11,
    //    TCM_QualityColor, // = 12,
    //    TCM_HalfHDR_Unsigned, // = 13,
    //    // TCM_HalfHDR_Signed, // = 14,
    //    // TCM_Max, // = 15,
    //    // TCM_Normals, // = 5,
    //    // TCM_NormalsHigh, // = 6,
    //    // TCM_NormalsGloss, // = 7,
    //    // TCM_HalfHDR, // = 13
    //}
}
