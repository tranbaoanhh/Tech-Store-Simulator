using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechStoreSimulator
{
    /// <summary>A lightweight demo customer that walks in, buys one stocked item, then leaves.</summary>
    public class CustomerAI : MonoBehaviour
    {
        [SerializeField] Vector3 checkoutPosition = new Vector3(0f, 0f, 2.2f);
        [SerializeField] Vector3 exitPosition = new Vector3(0f, 0f, -5f);
        [SerializeField, Min(0.1f)] float moveSpeed = 1.5f;
        [SerializeField, Min(0f)] float pauseAtCheckout = 1f;

        bool leaving;
        bool completed;

        void Start()
        {
            if (GameManager.Instance == null) { enabled = false; return; }
            StartCoroutine(VisitStore());
        }

        void Update()
        {
            if (completed) return;
            Vector3 destination = leaving ? exitPosition : checkoutPosition;
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            Vector3 direction = destination - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f) transform.forward = direction.normalized;
        }

        IEnumerator VisitStore()
        {
            yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.Products != null);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, checkoutPosition) < 0.05f);
            yield return new WaitForSeconds(pauseAtCheckout);

            List<Product> available = new List<Product>();
            foreach (Product product in GameManager.Instance.Products.Products)
                if (product != null && product.stock > 0 && product.price > 0) available.Add(product);

            if (available.Count == 0)
            {
                GameManager.Instance.SetStatus("Khach roi di: cua hang het hang.");
            }
            else
            {
                Product selected = available[Random.Range(0, available.Count)];
                GameManager.Instance.TrySell(selected.id, 1, out _, out _);
            }

            leaving = true;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, exitPosition) < 0.05f);
            completed = true;
            gameObject.SetActive(false);
        }
    }
}
