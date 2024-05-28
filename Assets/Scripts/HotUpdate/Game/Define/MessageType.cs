using UnityEngine;

public class MessageType
{
    //public struct CreateCharacterView
    //{
    //    public CharacterEntity character;
    //    public CharacterData characterData;
    //}

    //public struct CharacterVitalized
    //{
    //    public long entityID;
    //}

    public struct GameSceneCreating
    {
      
    }

    public struct GameSceneCreated
    {
       
    }

    //public struct CharacterAttributeChanged
    //{
    //    public long entityID;
    //    public CharacterAttributeType type;
    //    public long oldValue;
    //    public long newValue;
    //}

    public struct Login
    {
        public string name;
    }

    public struct LoginResp
    {

    }

    //public struct LoginResp
    //{
    //    public GameInfo gameInfo;
    //}

    //public struct EnterLobby
    //{
    //}

    //public struct EnterDungeon
    //{
    //}

    //public struct ExitDungeon
    //{

    //}

    //public struct EnterNextDungeon
    //{
    //}

    //public struct PickupItem
    //{
    //    public long entityID;
    //}

    //public struct SkillCastFinished
    //{
    //    public long entityID;
    //    public bool isCancel;
    //}

    //public struct SetInputEnableState
    //{
    //    public bool enabled;
    //}

    //public struct GameSceneLoadingStateChanged
    //{
    //    public bool isLoading;
    //}

    //public struct DropEquipment
    //{
    //    public Vector3 position;
    //    public Koakuma.Proto.EquipmentInfo equipmentInfo;
    //}

    //public struct PlayerDead
    //{
    //}

    //public struct ActiveMonsterSpawner
    //{
    //}

    //public struct WearEqupmentRequest
    //{
    //    public long itemID;
    //}

    //public struct TakeOffEquipmentRequest
    //{
    //    public Config.Parts parts;
    //}

    //public struct AddResource
    //{
    //    public ResourceType type;
    //    public long value;
    //}

    //public struct ResourceChanged
    //{
    //    public ResourceType type;
    //    public long oldValue;
    //    public long newValue;
    //}

    public struct NetConnected
    {
       
    }

    public struct NetDisconnected
    {
       
    }
}
