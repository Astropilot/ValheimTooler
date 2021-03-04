namespace ValheimTooler.Core.Extensions
{
    public static class MinimapExtensions
    {
        public static void VTExploreAll(this Minimap minimap)
        {
            if (minimap)
            {
                minimap.ExploreAll();
            }
        }
    }
}
