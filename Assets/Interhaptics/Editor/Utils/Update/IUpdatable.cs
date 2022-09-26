namespace Interhaptics.Editor.Utils.Updates
{
    /// <summary>
    /// Interface that allows the StartupWindow to check and display the update.
    /// </summary>
    public interface IUpdatable
    {
        #region Properties
        string Name { get; }
        bool HasUpdate { get; }
        #endregion

        #region Methods
        void Update();
        #endregion
    }
}
