public class NetPkgRandomSizesZA : NetPackage
{
    public static int LastEntityId = 0;
    public static float LastScale = 0f;

    public static void ResetInfo()
    {
        LastEntityId = 0;
        LastScale = 0f;
    }

    private int EntityId = 0;
    private float Scale = 0f;
    private float Min = 0f;
    private float Max = 0f;

    // Request server to answer with custom description
    // We currently abuse the same package for both ways
    // Could reduce the overhead a little by using either
    // A dedicated class or some dynamic switch flag 
    public NetPkgRandomSizesZA ToServer(int entityId, float min, float max)
    {
        EntityId = entityId;
        Scale = 0f;
        Min = min;
        Max = max;
        Log.Out("[RandomSizesZA Debug] PKG ToServer e:{0} min:{1} max:{2}", entityId, min, max);
        return this;
       
    }

    // Provide custom description to the client
    public NetPkgRandomSizesZA ToClient(int entityId, float scale)
    {
        EntityId = entityId;
        Scale = scale;
        Log.Out("[RandomSizesZA Debug] PKG ToClient e:{0} scale:{1} ", entityId, scale);
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        EntityId = _br.ReadInt32();
        Scale = _br.ReadSingle();
        Min = _br.ReadSingle();
        Max = _br.ReadSingle();
        Log.Out("[RandomSizesZA Debug] PKG Read");
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(EntityId);
        _bw.Write(Scale);
        _bw.Write(Min);
        _bw.Write(Max);
        Log.Out("[RandomSizesZA Debug] PKG Write");
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        Log.Out("[RandomSizesZA Debug] PKG ProcessPackge start");
        if (_world == null) return;
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            LastEntityId = EntityId;
            LastScale = Scale;
            Log.Out("[RandomSizesZA Debug] PKG ProcessPackge client e:{0} eL:{1} scale:{2} eS:{3}", EntityId, LastEntityId, Scale, LastScale);
        }
        else
        {
            // need to generate new scale to send to client.
            int entityId = EntityId;
            GameRandom random = new GameRandom();
            float scale = (float)(random.NextDouble() * (Max - Min) + Min);
            Log.Out("[RandomSizesZA Debug] PKG ProcessPackge server e:{0} scale:{1}", entityId, scale);
            // Send information to connected client
            Sender.SendPackage(NetPackageManager.GetPackage<NetPkgRandomSizesZA>().ToClient(entityId, scale));
        }
    }

    public override int GetLength() => 28;
}
