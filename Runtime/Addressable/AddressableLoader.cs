using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityMVC
{
    public static class AddressableLoader
    {
        public static async Task<AsyncOperationHandle> LoadAssetAsync<T>(string address) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);

            await handle.Task;

            return handle;
        }

        public static void ReleaseHanle(AsyncOperationHandle handle)
        {
            Addressables.Release(handle);
        }
    }
}