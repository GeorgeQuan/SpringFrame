public class EntityMessageType
{
    public struct SetGameSceneLoadingState
    {
        public bool isLoading;
    }

    //public struct AttachWeapon
    //{
    //    public WeaponEntity weaponEntity;
    //}

    //public struct DetachWeapon
    //{

    //}

    //public struct CastSkillRequest
    //{
    //    public int configID;
    //    public bool isCombo;
    //    public SkillTarget? target;
    //}

    //public struct CastSkillResponse : IEntityRpcResponse
    //{
    //    public bool Error { get; set; }
    //    public string ErrorMessage { get; set; }
    //    public bool comboImmediately;
    //}

    //public struct SkillInputTargetSatisfiedRequest
    //{
    //    public long targetID;
    //    public int skillID;
    //}

    //public struct SkillInputTargetSatisfiedResponse : IEntityRpcResponse
    //{
    //    public bool Error { set; get; }
    //    public string ErrorMessage { set; get; }
    //}

    //public struct SkillSelectTargetsRequest
    //{
    //    public List<SkillTarget> targetList;
    //    public SkillTarget? inputTarget;
    //    public SkillDetectType detectType;
    //    public float[] detectArgs;
    //    public SkillDetectCenterType detectCenterType;
    //    public float detectCenterOffset;
    //    public CharacterRelationType relation;
    //}

    //public struct SkillSelectTargetsResponse : IEntityRpcResponse
    //{
    //    public bool Error { get; set; }
    //    public string ErrorMessage { get; set; }
    //    public List<SkillTarget> targets;
    //}

    //public struct BeginSkillCasting
    //{
    //    public long casterID;
    //}

    //public struct EndSkillCasting
    //{
    //    public bool isCancel;
    //}

    //public struct UpdateMoveableState
    //{
    //}

    //public struct StopMove
    //{
    //}

    //public struct BindSkillInput
    //{
    //}

    //public struct SkillReachComboTime
    //{
    //}

    //public struct AddExp
    //{
    //    public long valueChange;
    //}

    //public struct ChangeAttribute
    //{
    //    public CharacterAttributeType type;
    //    public long valueChange;
    //}

    //public struct GetCharacterAttributeValueRequest
    //{
    //    public CharacterAttributeType key;
    //}

    //public struct GetCharacterAttributeValueResponse : IEntityRpcResponse
    //{
    //    public bool Error { get; set; }
    //    public string ErrorMessage { get; set; }
    //    public long value;
    //}

    //public struct GetHit
    //{
    //    public long attackerEntityID;
    //    public float attackPerct;
    //    public float attackAbso;
    //    public int fx;
    //    public int sfx;
    //}

    //public struct AddBuff
    //{
    //    public int configID;
    //    public long casterEntityID;
    //    public long parentEntityID;
    //    public BuffOverrideData? overrideData;
    //}

    //public struct RemoveAllBuffs { }

    //public struct StopAI { }

    //public struct TriggerBuff
    //{
    //    public BuffTriggerType triggerType;
    //}

    //public struct GenerateVibration
    //{
    //    public CameraVibrationStrength strength;
    //}

    //public struct CharacterVitalize
    //{
    //}

    //public struct MoveStateChanged
    //{
    //    public MoveState lastMoveState;
    //    public MoveState currentMoveState;
    //}

    //public struct CharacterViewCreated
    //{
    //    public CharacterViewComponent characterViewComponent;
    //}

    //public struct PathfindToTarget
    //{
    //    public Vector3 target;
    //    public bool force;
    //}

    //public struct StopPathfinding { }

    //public struct BulletHitEntity
    //{
    //    public long targetEntityID;
    //}

    //public struct BuffRemoved
    //{
    //    public long buffEntityID;
    //}

    //public struct RefreshWeaponBind
    //{
    //}

    //public struct ChangeHQModel
    //{
    //    public bool useHQ;
    //}

}
