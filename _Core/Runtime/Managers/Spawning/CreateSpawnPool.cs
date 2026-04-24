
namespace OGT
{
    using UnityEngine;

    public class CreateSpawnPool : GameBehavior, IAwake, IValidate
    {
        [SerializeField] private Spawnable spawnable;
        [SerializeField] private int initialCount;

        private SpawnManager spawnManager;

        public void OnAwake(Bootloader bootloader)
        {
            this.spawnManager = bootloader.FindManager<SpawnManager>();
            this.spawnManager.CreatePool(this.spawnable, this.initialCount);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.spawnable, nameof(this.spawnable));
        }

        private void OnDestroy()
        {
            if (this.spawnManager != null)
            {
                this.spawnManager.DestroyPool(this.spawnable);
            }
        }
    }
}
