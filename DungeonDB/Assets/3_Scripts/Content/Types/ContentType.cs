using System;

namespace Content
{
	[Serializable]
	public enum ContentType
	{
		Campaign			= 5,

		Story_Line			= 10,
		Story_Event,
		Story_Moment,

		World_Map			= 20,
		World_Location,
		World_Organization,

		Creature			= 30,
		//Creature_Player,
		//Creature_NPC,

		Custom				= 200,
		Unknown				= 300,
	}
}
