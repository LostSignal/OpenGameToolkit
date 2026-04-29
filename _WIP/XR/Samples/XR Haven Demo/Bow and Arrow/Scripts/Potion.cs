#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="Potion.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.Haven
{
    using OGT;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class Potion : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject plugObj;
        [SerializeField] private ParticleSystem particleSystemLiquid;
        [SerializeField] private ParticleSystem particleSystemSplash;
        [SerializeField] private float fillAmount = 0.8f;
        [SerializeField] private GameObject popVFX;

        [FormerlySerializedAs("meshRenderer")]
        [FormerlySerializedAs("MeshRenderer")]
        [SerializeField] private MeshRenderer potionMeshRenderer;

        [FormerlySerializedAs("SmashedObject")]
        [SerializeField] private GameObject smashedObject;

        [Header("Audio")]
        [SerializeField] private AudioBlock pouring;
        [SerializeField] private AudioBlock corkPop;
        [SerializeField] private AudioBlock potionBreaking;
#pragma warning disable 0649

        private AudioBlockInstance pouringInstance;
        private bool pugIn = true;
        private Rigidbody plugRigidbody;
        private MaterialPropertyBlock materialPropertyBlock;
        private Rigidbody rigidbodyPotion;
        private bool breakable;
        private float startingFillAmount;

        public void ToggleBreakable(bool breakable)
        {
            this.breakable = breakable;
        }

        public void PlugOff()
        {
            if (this.pugIn)
            {
                this.pugIn = false;
                this.plugRigidbody.transform.SetParent(null);
                this.plugRigidbody.isKinematic = false;
                this.plugRigidbody.AddRelativeForce(new Vector3(0, 0, 120));
                this.popVFX.SetActive(true);

                this.pugIn = false;

                this.plugObj.transform.parent = null;

                this.corkPop.PlayOneShotIfNotNull(this.plugRigidbody.transform.position);
            }
        }

        public void BreakPotion()
        {
            if (this.pugIn)
            {
                this.plugRigidbody.isKinematic = false;
                this.plugObj.transform.parent = null;

                if (this.plugObj.TryGetComponent(out Collider c))
                {
                    c.enabled = true;
                }

                GameObject.Destroy(this.plugObj, 4.0f);
            }

            foreach (Transform child in this.transform)
            {
                child.gameObject.SetActive(false);
            }

            if (this.particleSystemSplash != null)
            {
                this.particleSystemSplash.gameObject.SetActive(true);

                if (this.fillAmount > 0)
                {
                    this.particleSystemSplash.Play();
                }
            }

            this.smashedObject.SetActive(true);

            this.potionBreaking.PlayOneShotIfNotNull(this.transform.position);

            Rigidbody[] rbs = this.smashedObject.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rbs)
            {
                rb.AddExplosionForce(100.0f, this.smashedObject.transform.position, 2.0f, 15.0F);
            }

            GameObject.Destroy(this.smashedObject, 4.0f);
            GameObject.Destroy(this);
        }

        private void OnEnable()
        {
            this.particleSystemLiquid.Stop();

            if (this.particleSystemSplash)
            {
                this.particleSystemSplash.Stop();
            }

            this.materialPropertyBlock = new MaterialPropertyBlock();
            this.materialPropertyBlock.SetFloat("LiquidFill", this.fillAmount);

            this.potionMeshRenderer.SetPropertyBlock(this.materialPropertyBlock);
            this.plugRigidbody = this.plugObj.GetComponent<Rigidbody>();
            this.popVFX.SetActive(false);

            this.rigidbodyPotion = GetComponent<Rigidbody>();

            this.startingFillAmount = this.fillAmount;

            this.breakable = true;
        }

        private void Update()
        {
            if (Vector3.Dot(transform.up, Vector3.down) > 0 && this.fillAmount > 0 && this.pugIn == false)
            {
                if (this.particleSystemLiquid.isStopped)
                {
                    this.particleSystemLiquid.Play();
                    this.pouringInstance = this.pouring.PlayLooping();
                }

                this.fillAmount -= 0.1f * Time.deltaTime;

                float fillRatio = fillAmount / this.startingFillAmount;

                if (this.pouring != null)
                {
                    this.pouringInstance?.UpdatePitch(Mathf.Lerp(1.0f, 1.4f, 1.0f - fillRatio));
                }

                //// NOTE [bgish]: This was code from the old VR Beginner level for how to know if you're pouring the potion on something
                ////
                //// if (Physics.Raycast(this.particleSystemLiquid.transform.position, Vector3.down, out RaycastHit hit, 50.0f, ~0, QueryTriggerInteraction.Collide))
                //// {
                ////     PotionReceiver receiver = hit.collider.GetComponent<PotionReceiver>();
                ////
                ////     if (receiver != null)
                ////     {
                ////         receiver.ReceivePotion(potionType);
                ////     }
                //// }
            }
            else
            {
                this.particleSystemLiquid.Stop();
                this.pouringInstance?.Stop();
            }

            this.potionMeshRenderer.GetPropertyBlock(this.materialPropertyBlock);
            this.materialPropertyBlock.SetFloat("LiquidFill", this.fillAmount);
            this.potionMeshRenderer.SetPropertyBlock(this.materialPropertyBlock);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (this.breakable && this.rigidbodyPotion.linearVelocity.magnitude > 1.35)
            {
                if (this.pugIn)
                {
                    this.plugRigidbody.isKinematic = false;
                    this.plugObj.transform.parent = null;

                    if (this.plugObj.TryGetComponent(out Collider c))
                    {
                        c.enabled = true;
                    }

                    GameObject.Destroy(this.plugObj, 4.0f);
                }

                foreach (Transform child in this.transform)
                {
                    child.gameObject.SetActive(false);
                }

                if (this.particleSystemSplash != null)
                {
                    this.particleSystemSplash.gameObject.SetActive(true);

                    if (this.fillAmount > 0)
                    {
                        this.particleSystemSplash.Play();
                    }
                }

                this.smashedObject.SetActive(true);
                this.potionBreaking.PlayOneShotIfNotNull(this.transform.position);

                Rigidbody[] rbs = this.smashedObject.GetComponentsInChildren<Rigidbody>();

                foreach (Rigidbody rb in rbs)
                {
                    rb.AddExplosionForce(100.0f, this.smashedObject.transform.position, 2.0f, 15.0F);
                }

                GameObject.Destroy(this.smashedObject, 4.0f);
                GameObject.Destroy(this);
            }
        }
    }
}
