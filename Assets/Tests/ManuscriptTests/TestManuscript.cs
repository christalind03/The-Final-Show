using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestManuscript
{
    // A Test behaves as an ordinary method
    [Test]
    public void Test_Context_CollectScript()
    {
        GameplayTheme[] blankThemes = new GameplayTheme[0];
        GameplayContext context = new GameplayContext(blankThemes);

        context.CollectScript();

        Assert.AreEqual(1, context._scriptsCollected);
        Assert.AreEqual(1, context._lifetimeScriptsCollected);
    }
}
