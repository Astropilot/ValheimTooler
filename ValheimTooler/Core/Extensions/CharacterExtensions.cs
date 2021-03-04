namespace ValheimTooler.Core.Extensions
{
    public static class CharacterExtensions
    {
        public static void VTDamage(this Character character, float damage)
        {
            if (character != null)
            {
                HitData hitData = new HitData();

                hitData.m_damage.m_damage = damage;
                character.Damage(hitData);
            }
        }
    }
}
