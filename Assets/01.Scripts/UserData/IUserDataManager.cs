namespace _01.Scripts.UserData
{
    public interface IUserDataManager
    {
        T GetUserData<T>() where T : class, IUserData;
        void SaveUserData();
        bool IsUserDataLoaded();
    }
}