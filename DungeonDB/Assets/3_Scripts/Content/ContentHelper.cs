using System;
using System.Linq;

using Story;

namespace Content
{
	public static class ContentHelper
	{
		#region Types

		private class TypePair
		{
			public TypePair(ContentType _category, Type _type, string _subPath)
			{
				category = _category;
				type = _type;
				subPath = _subPath;
			}

			public ContentType category;
			public Type type;
			public string subPath = string.Empty;
		}

		#endregion
		#region Fields

		private static readonly TypePair[] typeCategoryPairs = new TypePair[]
		{
			new TypePair(ContentType.Campaign, typeof(Campaign), ""),

			new TypePair(ContentType.Story_Line, typeof(StoryLine), "Story/"),
			new TypePair(ContentType.Story_Event, typeof(StoryEvent), "Story/Events/"),
			new TypePair(ContentType.Story_Moment, typeof(StoryMoment), "Story/Events/"),

			new TypePair(ContentType.World_Map, typeof(WorldMap), "World/Maps/"),
			new TypePair(ContentType.World_Location, typeof(Location), "World/Locations/"),
			new TypePair(ContentType.World_Organization, typeof(Organization), "World/Organizations/"),

			new TypePair(ContentType.Creature, typeof(Creature), "Creatures/"),
			//new TypePair(ContentType.Creature_Player, typeof()),
			//new TypePair(ContentType.Creature_NPC, typeof()),

			new TypePair(ContentType.Custom, null, "Misc/"),
			new TypePair(ContentType.Unknown, null, "Misc/"),
		};

		#endregion
		#region Methods

		public static Type GetContentType(ContentType category)
		{
			return (from tp in typeCategoryPairs where tp.category == category select tp.type).FirstOrDefault();
		}

		public static ContentType GetContentCategory(Type type)
		{
			if (type == null) return ContentType.Unknown;

			return (from tp in typeCategoryPairs where tp.type == type select tp.category).FirstOrDefault();
		}

		public static string GetContentCategorySubPath(ContentType category)
		{
			return (from tp in typeCategoryPairs where tp.category == category select tp.subPath).FirstOrDefault();
		}

		#endregion
	}
}
