using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechStoreSimulator
{
    /// <summary>Demo customer: browses a stocked shelf, requests an item, pays, then leaves.</summary>
    public class CustomerAI : MonoBehaviour
    {
        [SerializeField] Vector3 checkoutPosition = new Vector3(0f, 1f, 2.2f);
        [SerializeField] Vector3 exitPosition = new Vector3(0f, 1f, -5f);
        [SerializeField, Min(0.1f)] float moveSpeed = 1.5f;
        [SerializeField, Min(0f)] float pauseAtShelf = 1.2f;
        [SerializeField, Min(0f)] float pauseAtCheckout = 0.8f;
        [SerializeField] int minBudget = 100000;
        [SerializeField] int maxBudget = 1000000;

        Vector3 destination;
        Customer customer;
        bool completed;

        void Start()
        {
            if (GameManager.Instance == null) { enabled = false; return; }
            customer = new Customer
            {
                id = System.Guid.NewGuid().ToString("N"),
                displayName = "Khach " + Random.Range(100, 999),
                budget = Random.Range(minBudget, maxBudget + 1)
            };
            destination = transform.position;
            StartCoroutine(VisitStore());
        }

        void Update()
        {
            if (completed) return;
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            Vector3 direction = destination - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f) transform.forward = direction.normalized;
        }

        IEnumerator VisitStore()
        {
            yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.Products != null);

            List<ProductInteraction> availableDisplays = new List<ProductInteraction>();
            foreach (ProductInteraction display in FindObjectsOfType<ProductInteraction>())
            {
                if (!display.gameObject.activeInHierarchy) continue;
                if (GameManager.Instance.Products.TryGet(display.ProductId, out Product product) && product.stock > 0)
                    availableDisplays.Add(display);
            }

            if (availableDisplays.Count == 0)
            {
                GameManager.Instance.SetStatus(customer.displayName + " roi di: cua hang het hang.");
                yield return LeaveStore();
                yield break;
            }

            ProductInteraction chosenDisplay = availableDisplays[Random.Range(0, availableDisplays.Count)];
            destination = new Vector3(chosenDisplay.transform.position.x, transform.position.y, chosenDisplay.transform.position.z - 0.9f);
            GameManager.Instance.SetStatus(customer.displayName + " dang xem " + chosenDisplay.name + ".");
            yield return new WaitUntil(() => Vector3.Distance(transform.position, destination) < 0.05f);
            yield return new WaitForSeconds(pauseAtShelf);

            destination = checkoutPosition;
            GameManager.Instance.SetStatus(customer.displayName + " mang hang den quay thanh toan.");
            yield return new WaitUntil(() => Vector3.Distance(transform.position, destination) < 0.05f);
            yield return new WaitForSeconds(pauseAtCheckout);

            bool success = GameManager.Instance.TrySell(chosenDisplay.ProductId, 1, customer.budget, out Order order, out string error);
            if (success)
                GameManager.Instance.SetStatus(customer.displayName + " mua " + chosenDisplay.name + " thanh cong: " + order.total.ToString("N0") + " VND.");
            else
                GameManager.Instance.SetStatus(customer.displayName + " khong mua duoc: " + error);

            yield return LeaveStore();
        }

        IEnumerator LeaveStore()
        {
            destination = exitPosition;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, destination) < 0.05f);
            completed = true;
            gameObject.SetActive(false);
        }
    }
}
