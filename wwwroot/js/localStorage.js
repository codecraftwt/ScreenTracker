window.localStorageHelper = {
    saveToLocalStorage: function (key, value) {
        console.log(`Saving to localStorage. Key: ${key}, Value: ${JSON.stringify(value)}`);
        localStorage.setItem(key, JSON.stringify(value)); // Store as JSON string
    },

    getFromLocalStorage: function (key) {
        const value = localStorage.getItem(key);
        console.log(`Retrieved from localStorage. Key: ${key}, Value: ${value}`);
        return value ? JSON.parse(value) : null; // Parse JSON string to object
    }
};
