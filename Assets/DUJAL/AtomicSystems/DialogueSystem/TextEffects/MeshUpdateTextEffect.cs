namespace DUJAL.Systems.Dialogue.Animations
{
    public class MeshUpdateTextEffect : TextEffect
    {
        public override void UpdateEffect() 
        {
            base.UpdateEffect();
            textComponent.ForceMeshUpdate();
        }
    }
}
