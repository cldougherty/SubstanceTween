using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class SubstanceTweenEditTest {

    public GameObject animatedObject;


	[Test]
	public void SubstanceTweenEditTestSimplePasses()
    {
        // Use the Assert class to test conditions.

        ProceduralMaterial crystalMaterial = Resources.Load("Resources/Testing/ProceduralMaterial/Crystal1-11") as ProceduralMaterial;


        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);


    }

	// A UnityTest behaves like a coroutine in PlayMode
	// and allows you to yield null to skip a frame in EditMode
	[UnityTest]

    public IEnumerator TestStartup()
    {
        //Assert.IsNotNull(animatedObject);
        ProceduralMaterial crystalMaterial = Resources.Load("Resources/Testing/ProceduralMaterial/Crystal1-11") as ProceduralMaterial;


        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        yield return new WaitForSeconds(20);
        yield break;
        //yield return null;
    }

	public IEnumerator SubstanceTweenEditTestWithEnumeratorPasses() {
		// Use the Assert class to test conditions.
		// yield to skip a frame
		yield return null;
	}
}
