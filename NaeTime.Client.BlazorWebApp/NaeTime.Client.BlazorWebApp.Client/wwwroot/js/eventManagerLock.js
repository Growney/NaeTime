const eventManagerLocks = new Map();

export function acquireEventManagerLock(lockId) {
    try {
        if (!lockId) return;
        if (navigator && navigator.locks && navigator.locks.request) {
            console.log("navigator.locks available; EventManager will attempt to acquire shared lock");

            let lockResolver;
            const promise = new Promise((res) => {
                console.log("Waiting to acquire shared lock for EventManager", lockId);
                lockResolver = res;
            });

            // store resolver so it can be released later
            eventManagerLocks.set(lockId, lockResolver);

            navigator.locks.request(lockId, { mode: "shared" }, () => {
                console.log("Acquired shared lock for EventManager", lockId);
                return promise;
            });
        }
        else {
            console.warn("navigator.locks not available; EventManager will proceed without lock");
        }
    }
    catch (err) {
        console.error('Failed to acquire EventManager lock', err);
    }
}

export function releaseEventManagerLock(lockId) {
    try {
        const resolver = eventManagerLocks.get(lockId);
        if (resolver) {
            resolver();
            eventManagerLocks.delete(lockId);
            console.log('Released EventManager lock', lockId);
        }
    }
    catch (err) {
        console.error('Failed to release EventManager lock', err);
    }
}
