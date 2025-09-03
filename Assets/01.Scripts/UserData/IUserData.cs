using System.Collections.Generic;

public interface IUserData
{
    bool IsLoaded { get; set; }
    void Initialize();
    void Setting(Dictionary<string, object> firestoreDict);
    void LoadData();
    void SaveData();
}
