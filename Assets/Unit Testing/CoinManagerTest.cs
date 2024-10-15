using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NewTestScript
{
    // Test to verify that the coin object is destroyed when collected 
    [UnityTest]
    public IEnumerator CoinIsDestroyedWhenCollected()
    {
        // Arrange: Create a coin game object
        GameObject coin = new GameObject("Coin");

        // Set the tag for the coin
        coin.tag = "Coin";

        // Destorying the Coin Object 
        Object.Destroy(coin);
        yield return null;  

        // Assert: Check if the coin has been destroyed
        Assert.IsTrue(coin == null);
    }
}
