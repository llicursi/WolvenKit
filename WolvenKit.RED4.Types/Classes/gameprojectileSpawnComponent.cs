using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	public partial class gameprojectileSpawnComponent : entIPlacedComponent
	{
		[Ordinal(5)] 
		[RED("spawnOffset")] 
		public Vector3 SpawnOffset
		{
			get => GetPropertyValue<Vector3>();
			set => SetPropertyValue<Vector3>(value);
		}

		[Ordinal(6)] 
		[RED("projectileTemplates")] 
		public CArray<CName> ProjectileTemplates
		{
			get => GetPropertyValue<CArray<CName>>();
			set => SetPropertyValue<CArray<CName>>(value);
		}

		[Ordinal(7)] 
		[RED("slotName")] 
		public CName SlotName
		{
			get => GetPropertyValue<CName>();
			set => SetPropertyValue<CName>(value);
		}

		public gameprojectileSpawnComponent()
		{
			Name = "Component";
			LocalTransform = new() { Position = new() { X = new(), Y = new(), Z = new() }, Orientation = new() { R = 1.000000F } };
			SpawnOffset = new();
			ProjectileTemplates = new();

			PostConstruct();
		}

		partial void PostConstruct();
	}
}
