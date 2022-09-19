namespace SharedScripts
{
    internal interface SharedISerializable
    {
        string Serialize();
        object DeSerialize();
    }
}
