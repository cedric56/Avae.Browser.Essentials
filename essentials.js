import { dotnetRuntime } from './main.js';
const fs = dotnetRuntime.Module.FS;

if (typeof fs.filesystems.IDBFS === 'undefined') {
    console.error("<EmccExtraLDFlags>-lidbfs.js</EmccExtraLDFlags> must be added to csproj");
}

fs.mkdir('/_cache', { recursive: true });
fs.mount(fs.filesystems.IDBFS, { autoPersist: true }, "/_cache");

fs.mkdir('/_appdata', { recursive: true });
fs.mount(fs.filesystems.IDBFS, { autoPersist: true }, "/_appdata");
fs.syncfs(true, function (err) { });
setInterval(() => {
    fs.syncfs(false, (err) => {
        if (err) console.error("Periodic sync failed:", err);
        //console.log("Persist");
    });
}, 5000); // Every 5 seconds

const exports = await dotnetRuntime.getAssemblyExports("Microsoft.Maui.Essentials");

export const databaseInterop = {
    fetch: function (url, data) {
        var xhr = new XMLHttpRequest();
        xhr.open("POST", url, false);
        xhr.onload = function () {
            console.log(this.responseText);
        };
        if (data !== null)
            //Send the proper header information along with the request
            xhr.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        xhr.send(data);

        if (xhr.status === 200) {
            return xhr.responseText;
        } else {
            throw new Error(xhr.statusText);
        }
    }
};

export const mediaCapture = {
    sendBlobToDotNet: async function (blobUrl) {
        const response = await fetch(blobUrl);
        const blob = await response.blob();

        const arrayBuffer = await blob.arrayBuffer();
        const byteArray = new Uint8Array(arrayBuffer);

        exports.Microsoft.Maui.Media.MediaPickerImplementation.ReceiveBlobData(Array.from(byteArray));
    },
    capturePhotoInPopup: function () {
        return new Promise((resolve, reject) => {
            const overlay = document.createElement("div");
            overlay.style.position = "fixed";
            overlay.style.top = 0;
            overlay.style.left = 0;
            overlay.style.width = "100%";
            overlay.style.height = "100%";
            overlay.style.background = "rgba(0,0,0,0.5)";
            overlay.style.zIndex = 9999;
            overlay.style.display = "flex";
            overlay.style.alignItems = "center";
            overlay.style.justifyContent = "center";

            const modal = document.createElement("div");
            modal.style.background = "#fff";
            modal.style.padding = "10px";
            modal.style.borderRadius = "8px";
            modal.style.width = "90%";
            modal.style.maxWidth = "480px";
            modal.style.textAlign = "center";

            modal.innerHTML = `
            <canvas id="photoCanvas" style="display:none;"></canvas>
            <div id="actionButtons" style="display:none; justify-content:center;">
                <button id="acceptPhoto">✅ Accept</button>
                <button id="rejectPhoto">❌ Reject</button>
            </div>
            <video id="videoPreview" autoplay playsinline style="width:100%; height:auto; border:1px solid #ccc;"></video>
            <button id="takePhoto" style="justify-content:center;">📸 Take Photo</button>
        `;

            overlay.appendChild(modal);
            document.body.appendChild(overlay);

            let videoStream;
            let photoTaken = false;

            const video = modal.querySelector("#videoPreview");
            const canvas = modal.querySelector("#photoCanvas");

            async function startCamera() {
                try {
                    // Use lower resolution for mobile
                    const constraints = /Android|iPhone/i.test(navigator.userAgent)
                        ? { video: { width: 320, height: 240 } }
                        : { video: { width: 640, height: 480 } };

                    videoStream = await navigator.mediaDevices.getUserMedia(constraints);
                    video.srcObject = videoStream;
                } catch (err) {
                    console.error("Camera access denied:", err);
                    cleanup(null);
                }
            }

            function takePhoto() {
                if (!videoStream) {
                    startCamera().then(() => {
                        takePhoto(); // restart after stream ready
                    });
                    return;
                }

                canvas.style.width = "100%";
                canvas.style.height = "auto";
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
                const ctx = canvas.getContext("2d");
                ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

                video.style.display = "none";
                canvas.style.display = "block";
                modal.querySelector("#actionButtons").style.display = "flex";
                modal.querySelector("#takePhoto").style.display = "none";
            }

            function acceptPhoto() {
                canvas.toBlob(blob => {
                    const imageUrl = URL.createObjectURL(blob);
                    photoTaken = true;
                    cleanup(imageUrl);
                }, "image/png");
            }

            function rejectPhoto() {
                canvas.style.display = "none";
                modal.querySelector("#actionButtons").style.display = "none";
                video.style.display = "block";
                modal.querySelector("#takePhoto").style.display = "block";
            }

            function cleanup(result) {
                if (videoStream) {
                    videoStream.getTracks().forEach(track => track.stop());
                }
                document.body.removeChild(overlay);
                resolve(result);
            }

            modal.querySelector("#takePhoto").addEventListener("click", takePhoto);
            modal.querySelector("#acceptPhoto").addEventListener("click", acceptPhoto);
            modal.querySelector("#rejectPhoto").addEventListener("click", rejectPhoto);

            overlay.addEventListener("click", e => {
                if (e.target === overlay) cleanup(null);
            });

            startCamera();
        });
    },
    captureVideoInPopup: function () {
        return new Promise((resolve) => {
            const overlay = document.createElement("div");
            overlay.style.position = "fixed";
            overlay.style.top = 0;
            overlay.style.left = 0;
            overlay.style.width = "100%";
            overlay.style.height = "100%";
            overlay.style.background = "rgba(0,0,0,0.5)";
            overlay.style.zIndex = 9999;
            overlay.style.display = "flex";
            overlay.style.alignItems = "center";
            overlay.style.justifyContent = "center";

            const modal = document.createElement("div");
            modal.style.background = "#fff";
            modal.style.padding = "10px";
            modal.style.borderRadius = "8px";
            modal.style.width = "90%";
            modal.style.maxWidth = "480px";
            modal.style.textAlign = "center";

            modal.innerHTML = `
<video id="videoPreview" autoplay playsinline muted style="width:100%;"></video>
<video id="playbackPreview" style="display:none;width:100%;"></video>
<div id="recordingControls" style="margin-top:10px;">
    <button id="startRecording">🎥 Start Recording</button>
    <button id="stopRecording" disabled>⏹ Stop</button>
</div>
<div id="actionButtons" style="display:none; justify-content:center; margin-top:10px;">
    <button id="acceptRecording">✅ Accept</button>
    <button id="rejectRecording">❌ Reject</button>
</div>
`;

            overlay.appendChild(modal);
            document.body.appendChild(overlay);

            let mediaRecorder;
            let recordedChunks = [];
            let videoStream;
            let videoAccepted = false;

            async function startCamera() {
                try {
                    const constraints = /Android|iPhone/i.test(navigator.userAgent)
                        ? {
                            audio: {
                                echoCancellation: true,
                                noiseSuppression: true,
                                autoGainControl: true
                            },
                            video: { width: 320, height: 240 }
                        }
                        : {
                            audio: {
                                echoCancellation: true,
                                noiseSuppression: true,
                                autoGainControl: true
                            },
                            video: { width: 640, height: 480 }
                        };

                    videoStream = await navigator.mediaDevices.getUserMedia(constraints);
                    modal.querySelector("#videoPreview").srcObject = videoStream;
                } catch (err) {
                    console.error("Camera access denied:", err);
                    cleanup(null);
                }
            }

            function startCapture() {
                if (videoStream) {
                    modal.querySelector("#startRecording").disabled = true;
                    modal.querySelector("#stopRecording").disabled = false;
                    modal.querySelector("#actionButtons").style.display = "none";

                    const options = { mimeType: "video/webm;codecs=vp8,opus" };
                    recordedChunks = [];
                    mediaRecorder = new MediaRecorder(videoStream, options);
                    mediaRecorder.ondataavailable = (event) => {
                        if (event.data.size > 0) {
                            recordedChunks.push(event.data);
                        }
                    };
                    mediaRecorder.onstop = showPlayback;
                    mediaRecorder.start();
                }
            }

            function stopCapture() {
                if (mediaRecorder && mediaRecorder.state !== "inactive") {
                    mediaRecorder.stop();
                }
                if (videoStream) {
                    videoStream.getTracks().forEach(track => track.stop());
                }
                modal.querySelector("#stopRecording").disabled = true;
                modal.querySelector("#recordingControls").style.display = "none";
            }

            function showPlayback() {
                const blob = new Blob(recordedChunks, { type: "video/webm;codecs=vp8,opus" });
                const url = URL.createObjectURL(blob);

                modal.querySelector("#videoPreview").style.display = "none";
                const playback = modal.querySelector("#playbackPreview");
                playback.src = url;
                playback.style.display = "block";
                playback.controls = true;
                playback.muted = false;
                playback.autoplay = false;

                modal.querySelector("#actionButtons").style.display = "flex";
            }

            function acceptCapture() {
                const blob = new Blob(recordedChunks, { type: "video/webm" });
                const url = URL.createObjectURL(blob);
                videoAccepted = true;
                cleanup(url);
            }

            function rejectCapture() {
                const playback = modal.querySelector("#playbackPreview");
                playback.muted = true;
                recordedChunks = [];
                startCamera();
                modal.querySelector("#playbackPreview").style.display = "none";
                modal.querySelector("#videoPreview").style.display = "block";
                modal.querySelector("#actionButtons").style.display = "none";
                modal.querySelector("#startRecording").disabled = false;
                modal.querySelector("#recordingControls").style.display = "flex";
            }

            function cleanup(result) {
                if (videoStream) {
                    videoStream.getTracks().forEach(track => track.stop());
                }
                if (overlay.parentNode) {
                    document.body.removeChild(overlay);
                }
                resolve(result);
            }

            // Event listeners
            modal.querySelector("#startRecording").addEventListener("click", startCapture);
            modal.querySelector("#stopRecording").addEventListener("click", stopCapture);
            modal.querySelector("#acceptRecording").addEventListener("click", acceptCapture);
            modal.querySelector("#rejectRecording").addEventListener("click", rejectCapture);

            overlay.addEventListener("click", e => {
                if (e.target === overlay) cleanup(null);
            });

            window.addEventListener("beforeunload", () => {
                if (!videoAccepted) cleanup(null);
            });

            startCamera();
        });
    }
};

export const mapInterop = {
    initMap: function (latitude, longitude, zoomLevel) {
        const platform = navigator.platform;

        if (/iPhone|iPad|iPod/.test(platform)) {
            // iOS: Open Apple Maps
            window.location.href = `maps://?q=${latitude},${longitude}`;
        } else if (/Android/.test(platform)) {
            // Android: Open Google Maps or default Maps app
            window.location.href = `geo:${latitude},${longitude}?q=${latitude},${longitude}`;
        } else {
            // Fallback: Open Google Maps in browser (for other platforms)
            const url = `https://www.google.com/maps?q=${latitude},${longitude}`;
            window.open(url, '_blank');
        }
    }
};


export const authInterop = {
    authenticate: function (authUrl, redirectUri) {
        return new Promise((resolve, reject) => {

            const popup = window.open(authUrl, "AuthPopup");

            const channel = new BroadcastChannel('token');
            channel.onmessage = (event) => {
                resolve(event.data);
                channel.close();
                popup.close();
            }
        });
    }
};

export const textToSpeechInterop = {
    speak: function (text) {
        if (!window.speechSynthesis) {
            console.error("❌ Speech Synthesis API is not supported in this browser.");
            return;
        }

        let utterance = new SpeechSynthesisUtterance(text);
        window.speechSynthesis.speak(utterance);

        console.log("✅ Speaking:", text);
    },
    speakWithOptions: function (text, lang, voice, volume, pitch) {
        if (!window.speechSynthesis) {
            console.error("❌ Speech Synthesis API is not supported in this browser.");
            return;
        }

        this.resolveVoices().then(voices => {
            let utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = lang;
            utterance.voice = voices.find(v => v.voiceURI == voice);
            utterance.volume = volume;
            utterance.pitch = pitch;
            window.speechSynthesis.speak(utterance);
            console.log("✅ Speaking:", text);
        });
    },
    getVoices: function () {
        return new Promise(async (resolve, reject) => {
            if (!window.speechSynthesis) {
                console.error("❌ Speech Synthesis API is not supported in this browser.");
                reject(null);
                return;
            }
            this.resolveVoices().then(voices => {
                resolve(JSON.stringify(voices.map(voice => ({
                    name: voice.name,
                    lang: voice.lang,
                    default: voice.default,
                    voiceURI: voice.voiceURI,
                    localService: voice.localService
                })), null, 2));
            });
        });
    },
    resolveVoices: function () {
        return new Promise((resolve) => {
            let voices = window.speechSynthesis.getVoices();
            if (voices.length !== 0) {
                resolve(voices);
            }
            else {
                window.speechSynthesis.addEventListener("voiceschanged", function () {
                    voices = window.speechSynthesis.getVoices();
                    resolve(voices);
                });
            }
        });
    }
};

export const connectivityInterop = {
    isOnline: function () {
        return navigator.onLine;
    },
    getConnectionType: function () {
        var connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        return connection.type ?? connection.effectiveType;
    },
    onStatusChange: function () {
        function updateStatus() {
            exports.Microsoft.Maui.Networking.ConnectivityImplementation.OnConnectivityChanged(navigator.onLine)
        }

        window.addEventListener("online", updateStatus);
        window.addEventListener("offline", updateStatus);
    }
};

export const contactsInterop = {
    getAllAsync: async function (multiple) {
        return new Promise(async (resolve, reject) => {
            if ('ContactsManager' in window) {

                const opts = { multiple: multiple };
                const contacts = await navigator.contacts.select(["name", "email", "tel", "address"], opts);
                const contactsJson = contacts.map(voice => ({
                    name: voice.name,
                    email: voice.email,
                    tel: voice.tel,
                    address: voice.address,
                }));
                resolve(JSON.stringify(contactsJson, null, 2));
                return;
            }

            resolve('');

        });
    }
};

export const compassInterop = {
    frequency: null, // Hz (updates per second)
    lastUpdateTime: null,
    startListening: function (frequency) {
        this.frequency = frequency || 5; // Default to 5 Hz if not provided
        if (typeof window.DeviceOrientationEvent !== 'undefined' &&
            typeof window.DeviceOrientationEvent.requestPermission === 'function') {
            // iOS 13+ requires permission
            window.DeviceOrientationEvent.requestPermission()
                .then(permissionState => {
                    if (permissionState === 'granted') {
                        window.addEventListener('deviceorientation', this.handle);
                    } else {
                        console.warn('Permission to access device orientation denied.');
                    }
                })
                .catch(console.error);
        } else {
            // Non iOS devices
            window.addEventListener('deviceorientationabsolute', this.handle);
        }
    },
    stopListening: function () {
        window.removeEventListener('deviceorientation', this.handle);
        window.removeEventListener('deviceorientationabsolute', this.handle);
    },
    handle: function (event) {

        if (event.alpha === null) return; // no data

        const now = Date.now();
        const interval = 1000 / this.frequency;

        if (now - this.lastUpdateTime < interval) return;
        this.lastUpdateTime = now;

        // Normalize alpha to 0-360 degrees
        const heading = event.webkitCompassHeading || Math.abs(event.alpha - 360);

        exports.Microsoft.Maui.Devices.Sensors.CompassImplementation.OnReadingChanged(
            heading
        );
    }
};

export const accelerometerInterop = {
    frequency: 10,
    lastUpdateTime: 0,
    startListening: function (frequency) {

        this.frequency = frequency;
        if (window.DeviceOrientationEvent != undefined) {

            function onDeviceMotion(event) {
                const now = Date.now();
                const interval = 1000 / this.frequency;

                if (now - this.lastUpdateTime < interval) return;
                this.lastUpdateTime = now;

                exports.Microsoft.Maui.Devices.Sensors.AccelerometerImplementation.OnReadingChanged(
                    event.accelerationIncludingGravity.x || 0,
                    event.accelerationIncludingGravity.y || 0,
                    event.accelerationIncludingGravity.z || 0
                );
            }

            if (typeof DeviceOrientationEvent.requestPermission === 'function') {
                DeviceOrientationEvent.requestPermission()
                    .then(permissionState => {
                        if (permissionState === 'granted') {
                            window.ondevicemotion = onDeviceMotion;
                        }
                    })
                    .catch(console.error);
            }
            else {
                window.ondevicemotion = onDeviceMotion;
            }
        }
    },
    stopListening: function () {

    }
};

export const magnetometerInterop = {
    magSensor: null,
    startListening: function (frequency) {
        if ('magnetometer' in navigator) {
            this.magSensor = new Magnetometer({ frequency: frequency });
            this.magSensor.addEventListener("reading", this.handle);
            this.magSensor.start();
        }
    },
    stopListening: function () {
        if (this.magSensor !== null) {
            this.magSensor.removeEventListener("reading", this.handle);
            this.magSensor.stop();
        }
    },
    handle: function (e) {
        exports.Microsoft.Maui.Devices.Sensors.MagnetometerImplementation.OnReadingChanged(
            this.magSensor.x || 0,
            this.magSensor.y || 0,
            this.magSensor.z || 0
        );
    }
};

export const gyroscopeInterop = {
    frequency: 15,
    gyroscope: null,
    startListening: function (frequency) {

        this.frequency = frequency;
        if ('Gyroscope' in navigator) {
            this.gyroscope = new Gyroscope({ frequency: frequency });
            this.gyroscope.addEventListener("reading", this.handle);
            this.gyroscope.start();
        }
        else {
            if (window.DeviceOrientationEvent != undefined) {

                function onDeviceMotion(event) {
                    const now = Date.now();
                    const interval = 1000 / this.frequency;

                    if (now - this.lastUpdateTime < interval) return;
                    this.lastUpdateTime = now;

                    exports.Microsoft.Maui.Devices.Sensors.GyroscopeImplementation.OnReadingChanged(
                        event.rotationRate.alpha || 0,
                        event.rotationRate.beta || 0,
                        event.rotationRate.gamma || 0
                    );
                }

                if (typeof DeviceOrientationEvent.requestPermission === 'function') {
                    DeviceOrientationEvent.requestPermission()
                        .then(permissionState => {
                            if (permissionState === 'granted') {
                                window.ondevicemotion = onDeviceMotion;
                            }
                        })
                        .catch(console.error);
                }
                else {
                    window.ondevicemotion = onDeviceMotion;
                }
            }
        }
    },
    stopListening: function () {
        if (this.gyroscope !== null) {
            this.gyroscope.removeEventListener("reading", this.handle);
            this.gyroscope.stop();
        }
    },
    handle: function (e) {
        exports.Microsoft.Maui.Devices.Sensors.GyroscopeImplementation.OnReadingChanged(
            this.gyroscope.x || 0,
            this.gyroscope.y || 0,
            this.gyroscope.z || 0
        );
    }
};

export const orientationInterop = {
    lastUpdateTime: null,
    sensor: null,
    startListening: function (frequency) {
        if ('AbsoluteOrientationSensor' in window) {
            this.sensor = new AbsoluteOrientationSensor({ frequency: frequency });
            this.sensor.addEventListener('reading', this.handle);
            this.sensor.start();
        }
        else {
            if (window.DeviceOrientationEvent != undefined) {

                function onDeviceOrientation(event) {
                    const now = Date.now();
                    const interval = 1000 / frequency;

                    if (now - this.lastUpdateTime < interval) return;
                    this.lastUpdateTime = now;

                    exports.Microsoft.Maui.Devices.Sensors.OrientationSensorImplementation.OnReadingChanged(
                        event.alpha || 0,
                        event.beta || 0,
                        event.gamma || 0,
                        0
                    );
                }

                if (typeof DeviceOrientationEvent.requestPermission === 'function') {
                    DeviceOrientationEvent.requestPermission()
                        .then(permissionState => {
                            if (permissionState === 'granted') {
                                window.ondeviceorientation = onDeviceOrientation;
                            }
                        })
                        .catch(console.error);
                }
                else {
                    window.ondeviceorientation = onDeviceOrientation;
                }
            }
        }
    },
    stopListening: function () {
        if (this.sensor)
            this.sensor.removeEventListener("reading", this.handle);
    },
    handle: function (event) {
        exports.Microsoft.Maui.Devices.Sensors.OrientationSensorImplementation.OnReadingChanged(
            sensor.quaternion[0] || 0,
            sensor.quaternion[1] || 0,
            sensor.quaternion[2] || 0,
            sensor.quaternion[3]
        );
    }
};

export const geolocationInterop = {
    getCurrentLocation: async function () {

        return new Promise((resolve, reject) => {
            // Check if geolocation is available
            if ("geolocation" in navigator) {
                navigator.geolocation.getCurrentPosition(
                    // Success callback
                    function (position) {
                        const location = {
                            latitude: position.coords.latitude,
                            longitude: position.coords.longitude,
                            accuracy: position.coords.accuracy,
                            success: true
                        };
                        resolve(JSON.stringify(location));
                    },
                    // Error callback
                    function (error) {
                        reject(JSON.stringify({
                            success: false,
                            message: `Error: ${error.message}`,
                            errorCode: error.code
                        }));
                    }
                );
            } else {
                // Reject if geolocation is not supported
                reject(JSON.stringify({
                    success: false,
                    message: "Geolocation is not supported by this browser."
                }));
            }
        });


    }
};

export const batteryInterop = {
    getBatteryStatus: async function () {
        if (!navigator.getBattery) {
            return { success: false, error: "Battery API not supported in this browser" };
        }
        const battery = await navigator.getBattery();

        function handleBattery(event) {
            exports.Microsoft.Maui.Devices.BatteryImplementation.OnBatteryChanged(
                battery.level,  // Convert 0-1 to percentage
                battery.charging,
                battery.chargingTime,
                battery.dischargingTime
            );
        }

        battery.addEventListener("chargingchange", handleBattery);
        battery.addEventListener("levelchange", handleBattery);
        battery.addEventListener("chargingtimechange", handleBattery);
        battery.addEventListener("dischargingtimechange", handleBattery);

        try {

            return JSON.stringify({
                success: true,
                level: battery.level,  // Convert 0-1 to percentage
                charging: battery.charging,
                chargingTime: battery.chargingTime,
                dischargingTime: battery.dischargingTime
            });
        } catch (error) {
            return JSON.stringify({
                success: false,
                error: error.message
            });
        }
    }
};

export const vibrationInterop = {
    vibrateWithDuration: function (duration) {
        if (window.navigator.vibrate !== undefined) {
            navigator.vibrate(duration);
            return true;  // Return success
        } else {
            console.error("Vibration API is not supported in this browser.");
            return false; // Return failure
        }
    },
    vibrate: function () {
        if (window.navigator.vibrate !== undefined) {
            window.navigator.vibrate(200);
            return true;  // Return success
        } else {
            console.error("Vibration API is not supported in this browser.");
            return false; // Return failure
        }
    },
    cancel: function () {
        if (window.navigator.vibrate !== undefined) {
            navigator.vibrate(0);
            return true;  // Return success
        } else {
            console.error("Vibration API is not supported in this browser.");
            return false; // Return failure
        }
    }
};

export const semanticScreenReaderInterop = {
    announce: function (message) {
        let liveRegion = document.getElementById("live-region");
        if (!liveRegion) {
            liveRegion = document.createElement("div");
            liveRegion.id = "live-region";
            liveRegion.setAttribute("aria-live", "assertive"); // Announce immediately
            liveRegion.setAttribute("aria-atomic", "true");  // Always announce the latest change
            liveRegion.style.position = "absolute";
            liveRegion.style.width = "1px";
            liveRegion.style.height = "1px";
            liveRegion.style.overflow = "hidden";
            document.body.appendChild(liveRegion);
        }
        liveRegion.textContent = message;
    }
};

//export const permissionsInterop = {
//    checkPermission: async function (permissionName) {
//        if (!navigator.permissions) {
//            console.error("Permissions API is not supported in this browser.");
//            return "not_supported";
//        }

//        try {
//            const result = await navigator.permissions.query({ name: permissionName });
//            return result.state; // "granted", "denied", or "prompt"
//        } catch (error) {
//            console.error("Error checking permission:", error);
//            return "error";
//        }
//    },
//    requestPermission: async function (permissionName) {
//        if (permissionName === "notifications") {
//            return await Notification.requestPermission(); // "granted", "denied", "default"
//        }

//        if (permissionName === "geolocation") {
//            return new Promise((resolve) => {
//                navigator.geolocation.getCurrentPosition(
//                    () => resolve("granted"),
//                    () => resolve("denied")
//                );
//            });
//        }

//        console.error("Permission request not supported for:", permissionName);
//        return "not_supported";
//    }
//};

export const preferencesInterop = {
    setItem: function (key, value) {
        try {
            localStorage.setItem(key, value);
            return true;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return false;
        }
    },

    getItem: function (key) {
        try {
            return localStorage.getItem(key) || null;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return null;
        }
    },

    removeItem: function (key) {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return false;
        }
    },
    clear: function (key) {
        try {
            localStorage.clear();
            return true;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return false;
        }
    }
};

// Helper: Convert base64 to ArrayBuffer
function base64ToArrayBuffer(base64) {
    const binaryString = atob(base64);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) bytes[i] = binaryString.charCodeAt(i);
    return bytes.buffer;
}

// Helper: Convert ArrayBuffer to base64
function arrayBufferToBase64(buffer) {
    const bytes = new Uint8Array(buffer);
    const binary = bytes.reduce((acc, b) => acc + String.fromCharCode(b), '');
    return btoa(binary);
}

// Derive a key from a passphrase
async function deriveKey(password) {
    const enc = new TextEncoder();
    const keyMaterial = await crypto.subtle.importKey(
        "raw",
        enc.encode(password),
        { name: "PBKDF2" },
        false,
        ["deriveKey"]
    );

    return crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: enc.encode("fixed-salt"), // Replace with random salt for production
            iterations: 100000,
            hash: "SHA-256"
        },
        keyMaterial,
        { name: "AES-GCM", length: 256 },
        false,
        ["encrypt", "decrypt"]
    );
}

// Encrypt text with password
export async function encrypt(text, password) {
    const key = await deriveKey(password);
    const enc = new TextEncoder();
    const iv = crypto.getRandomValues(new Uint8Array(12));
    const data = enc.encode(text);

    const encrypted = await crypto.subtle.encrypt(
        { name: "AES-GCM", iv },
        key,
        data
    );

    return JSON.stringify({
        iv: arrayBufferToBase64(iv),
        data: arrayBufferToBase64(encrypted)
    });
}

// Decrypt encrypted payload
export async function decrypt(payloadJson, password) {
    const { iv, data } = JSON.parse(payloadJson);
    const key = await deriveKey(password);

    const decrypted = await crypto.subtle.decrypt(
        {
            name: "AES-GCM",
            iv: base64ToArrayBuffer(iv)
        },
        key,
        base64ToArrayBuffer(data)
    );

    const dec = new TextDecoder();
    return dec.decode(decrypted);
}

export const secureStorageInterop = {
    setItem: function (key, value) {
        try {
            localStorage.setItem(key, value);
            return true;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return false;
        }
    },

    getItem: function (key) {
        try {
            return localStorage.getItem(key) || null;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return null;
        }
    },

    removeItem: function (key) {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return false;
        }
    },
    clear: function (key) {
        try {
            localStorage.clear();
            return true;
        } catch (error) {
            console.error("Secure Storage Error:", error);
            return false;
        }
    }
};

let wakeLock = null;
export const deviceDisplayInterop = {
    get: function () {

        function handler() {

            exports.Microsoft.Maui.Devices.DeviceDisplayImplementation.OnDeviceDisplayChanged();

        }

        window.addEventListener("resize", handler);
        return JSON.stringify({
            width: window.screen.width,
            height: window.screen.height,
            isLandscape: window.matchMedia("(orientation: landscape)").matches,
            density: window.devicePixelRatio,
            rotation: screen.orientation ? screen.orientation.angle : 0
        });
    },
    requestWakeLock: async function () {
        try {
            wakeLock = await navigator.wakeLock.request('screen');
            console.log('Screen Wake Lock active');

            // Re-acquire the lock on visibilitychange
            document.addEventListener('visibilitychange', async () => {
                if (wakeLock !== null && document.visibilityState === 'visible') {
                    wakeLock = await navigator.wakeLock.request('screen');
                }
            });
        } catch (err) {
            console.error(`${err.name}, ${err.message}`);
        }
    },
    releaseWakeLock: function () {
        if (wakeLock !== null) {
            wakeLock.release();
            wakeLock = null;
            console.log('Screen Wake Lock released');
        }
    }
};

export const flashInterop = {
    on: async function () {
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            await navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: 'environment', // This targets the rear camera
                    torch: true // If flashlight is off, turn it on and vice versa
                }
            });
        }
    },

    off: async function () {
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            await navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: 'environment', // This targets the rear camera
                    torch: false // If flashlight is off, turn it on and vice versa
                }
            });
        }
    }
};


export const browserDetect = {
    get: async function () {
        const { default: UAParser } = await import("https://cdn.jsdelivr.net/npm/ua-parser-js@1.0.2/+esm");

        var parser = new UAParser();
        var result = parser.getResult();

        var gl = getGlRenderer();

        var isMobile = navigator.userAgent.toLowerCase().indexOf('mobile') > -1 ?? (isiOS()),
            isTablet = navigator.userAgent.toLowerCase().indexOf('tablet') > -1 ?? (isiPadOS() || isiPadPro()),
            isAndroid = navigator.userAgent.toLowerCase().indexOf('android') > -1,
            isiPhone = isiOS(),
            isiPad = isiPadOS() || isiPadPro();

        var agent = window.navigator.userAgent;
        var timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

        var deviceVendor = result.device.vendor ?? '';
        if (deviceVendor === '' && (isiPad || isiPhone || isiPadPro()))
            deviceVendor = 'Apple Inc.';

        var deviceModel = result.device.model ?? '';
        var deviceType = result.device.type ?? '';

        console.log('Device model ->' + deviceModel);
        if (deviceModel === 'iPad' || isiPad) {
            deviceType = 'iPad';
            deviceModel = getModels().toString();
        }
        else if (deviceModel === 'iPhone' || isiPhone) {
            deviceType = 'iPhone';
            deviceModel = getModels().toString();
        }

        var osName = parser.getOS().name;
        var osVersion = parser.getOS().version;

        var winVer = '';

        if (typeof navigator.userAgentData !== 'undefined') {
            navigator.userAgentData.getHighEntropyValues(["platformVersion"])
                .then(ua => {
                    if (navigator.userAgentData.platform === "Windows") {
                        var majorPlatformVersion = parseInt(ua.platformVersion.split('.')[0]);
                        if (majorPlatformVersion >= 13) {
                            winVer = "11 or later";
                        }
                        else {
                            switch (majorPlatformVersion) {
                                case 0:
                                    winVer = "7/8/8.1";
                                    break;
                                case 1:
                                    winVer = "10 (1507)";
                                    break;
                                case 2:
                                    winVer = "10 (1511)";
                                    break;
                                case 3:
                                    winVer = "10 (1607)";
                                    break;
                                case 4:
                                    winVer = "10 (1703)";
                                    break;
                                case 5:
                                    winVer = "10 (1709)";
                                    break;
                                case 6:
                                    winVer = "10 (1803)";
                                    break;
                                case 7:
                                    winVer = "10 (1809)";
                                    break;
                                case 8:
                                    winVer = "10 (1903 or 10 1909)";
                                    break;
                                case 10:
                                    winVer = "10 (2004 or 20H2 or 21H1 or 21H2)";
                                    break;
                                default:
                                    break;
                            }
                        }

                        osVersion = winVer;

                        exports.Microsoft.Maui.Devices.DeviceInfoImplementation.OnOSUpdate(winVer);
                        //dotNetObjectRef.invokeMethodAsync('OSUpdateString', winVer).then(null, function (err) {
                        //    throw new Error(err);
                        //});
                    }
                    else if (navigator.userAgentData.platform === "macOS") {
                        var macVer = ua.platformVersion;
                        exports.Microsoft.Maui.Devices.DeviceInfoImplementation.OnOSUpdate(macVer);
                        //dotNetObjectRef.invokeMethodAsync('OSUpdateString', macVer).then(null, function (err) {
                        //    throw new Error(err);
                        //});
                    }
                });

            navigator.userAgentData.getHighEntropyValues(["architecture", "bitness"])
                .then(ua => {
                    if (navigator.userAgentData.platform === "Windows") {
                        var winCPU = '';
                        if (ua.architecture === 'x86') {
                            if (ua.bitness === '64') {
                                winCPU = "x86_64";
                            }
                            else if (ua.bitness === '32') {
                                winCPU = "x86";
                            }
                        }
                        else if (ua.architecture === 'arm') {
                            if (ua.bitness === '64') {
                                winCPU = "ARM64";
                            }
                            else if (ua.bitness === '32') {
                                winCPU = "ARM32";
                            }
                        }

                        exports.Microsoft.Maui.Devices.DeviceInfoImplementation.OnOSArchitecture(winCPU);

                        //dotNetObjectRef.invokeMethodAsync('OSArchitectureUpdateString', winCPU).then(null, function (err) {
                        //    throw new Error(err);
                        //});
                    }
                    else if (navigator.userAgentData.platform === "macOS") {
                        var macCPU = ua.architecture + ' ' + ua.bitness;
                        exports.Microsoft.Maui.Devices.DeviceInfoImplementation.OnOSArchitecture(macCPU);

                        //dotNetObjectRef.invokeMethodAsync('OSArchitectureUpdateString', macCPU).then(null, function (err) {
                        //    throw new Error(err);
                        //});
                    }
                });
        }

        var rtn = {
            BrowserMajor: result.browser.major ?? '',
            BrowserName: result.browser.name ?? '',
            BrowserVersion: result.browser.version ?? '',
            CPUArchitect: result.cpu.architecture ?? '',
            DeviceModel: deviceModel,
            DeviceType: deviceType,
            DeviceVendor: deviceVendor,
            EngineName: result.engine.name ?? '',
            EngineVersion: result.engine.version ?? '',
            GPURenderer: gl.glRenderer,
            GPUVendor: gl.glVendor,
            IsDesktop: isDesktop(),
            IsMobile: isMobile ?? false,
            IsTablet: isTablet ?? false,
            IsAndroid: isAndroid ?? false,
            IsIPhone: isiPhone ?? false,
            IsIPad: isiPad ?? false,
            isIpadPro: isiPadPro(),
            OSName: osName,
            OSVersion: osVersion,
            ScreenResolution: (window.screen.width * window.devicePixelRatio) + 'x' + (window.screen.height * window.devicePixelRatio),
            TimeZone: timeZone,
            UserAgent: agent
        };

        return JSON.stringify(rtn);
    }
}

function isiOS() {
    if (/iPhone|iPod/.test(navigator.platform)) {
        return true;
    } else {
        return false;
    }
}

function isiPadOS() {
    if (/iPad/.test(navigator.platform)) {
        return true;
    } else {
        return false;
    }
}

function isiPadPro() {
    var ratio = window.devicePixelRatio || 1;
    var screen = {
        width: window.screen.width * ratio,
        height: window.screen.height * ratio
    };

    return (screen.width === 2048 && screen.height === 2732) ||
        (screen.width === 2732 && screen.height === 2048) ||
        (screen.width === 1536 && screen.height === 2048) ||
        (screen.width === 2048 && screen.height === 1536);
}

function isDesktop() {
    if ((navigator.userAgent.match(/iPhone/i)) ||
        (navigator.userAgent.match(/(up.browser|up.link|mmp|symbian|smartphone|midp|wap|vodafone|o2|pocket|kindle|mobile|pda|psp|treo)/i)) ||
        (navigator.userAgent.match(/iPod/i)) ||
        (navigator.userAgent.match(/operamini/i)) ||
        (navigator.userAgent.match(/blackberry/i)) ||
        (navigator.userAgent.match(/(palmos|palm|hiptop|avantgo|plucker|xiino|blazer|elaine)/i)) ||
        (navigator.userAgent.match(/(windowsce; ppc;|windows ce;smartphone;|windows ce; iemobile) /i)) ||
        (navigator.userAgent.match(/android/i)) || isiOS() || isiPadOS() || isiPadPro()) {
        return false;
    }
    else {
        return true;
    }
}

function OSVersion() {
    var rtn = "";

    var userAgent = window.navigator.userAgent;

    if (userAgent.indexOf("Windows NT 10.0") > 0) {
        rtn = "Windows 10/11";
    }
    else if (userAgent.indexOf("Windows NT 6.3") > 0) {
        rtn = "Windows 8.1";
    }
    else if (userAgent.indexOf("Windows NT 6.2") > 0) {
        rtn = "Windows 8";
    }
    else if (userAgent.indexOf("Windows NT 6.1") > 0) {
        rtn = "Windows 7";
    }
    else if (userAgent.indexOf("Windows NT 6.0") > 0) {
        rtn = "Windows Vista";
    }
    else if (userAgent.indexOf("Windows NT 5.2") > 0) {
        rtn = "Windows Server 2003; Windows XP x64 Edition";
    }
    else if (userAgent.indexOf("Windows NT 5.1") > 0) {
        rtn = "Windows XP";
    }
    else if (userAgent.indexOf("Windows NT 5.01") > 0) {
        rtn = "Windows 2000, Service Pack 1 (SP1)";
    }
    else if (userAgent.indexOf("Windows NT 5.0") > 0) {
        rtn = "Windows 2000";
    }
    else if (userAgent.indexOf("Windows NT 4.0") > 0) {
        rtn = "Microsoft Windows NT 4.0";
    }
    else if (userAgent.indexOf("Win 9x 4.90") > 0) {
        rtn = "Windows Millennium Edition (Windows Me)";
    }
    else if (userAgent.indexOf("Windows 98") > 0) {
        rtn = "Windows 98";
    }
    else if (userAgent.indexOf("Windows 95") > 0) {
        rtn = "Windows 95";
    }
    else if (userAgent.indexOf("Windows CE") > 0) {
        rtn = "Windows CE";
    }
    else if (userAgent.indexOf("iPhone OS") > 0) {
        rtn = "iPhone OS";
    }
    else if (userAgent.indexOf("Mac OS") > 0) {
        rtn = "Max OS";
    }
    else if (userAgent.indexOf("Android") > 0) {
        rtn = "Android";
    }
    else if (userAgent.indexOf("Silk") > 0) {
        rtn = "Amazon Fire";
    }
    else if (userAgent.indexOf("facebook") > 0) {
        rtn = "Facebook External Hit";
    }
    else if (userAgent.indexOf("Twitterbot") > 0) {
        rtn = "Twitterbot";
    }
    else if (userAgent.indexOf("WhatsApp") > 0) {
        rtn = "WhatsApp";
    }
    else {
        //Others
    }

    return rtn;
}

var canvas, gl, glRenderer, models,
    devices = [
        ['a7', '640x1136', ['iPhone 5', 'iPhone 5s']],
        ['a7', '1536x2048', ['iPad Air', 'iPad Mini 2', 'iPad Mini 3', 'iPad Pro 9.7']],
        ['a8', '640x1136', ['iPod touch (6th gen)']],
        ['a8', '750x1334', ['iPhone 6']],
        ['a8', '1242x2208', ['iPhone 6 Plus']],
        ['a8', '1536x2048', ['iPad Air 2', 'iPad Mini 4']],
        ['a9', '640x1136', ['iPhone SE']],
        ['a9', '750x1334', ['iPhone 6s']],
        ['a9', '1242x2208', ['iPhone 6s Plus']],
        ['a9x', '1536x2048', ['iPad Pro (1st gen 9.7-inch)']],
        ['a9x', '2048x2732', ['iPad Pro (1st gen 12.9-inch)']],
        ['a10', '750x1334', ['iPhone 7']],
        ['a10', '1242x2208', ['iPhone 7 Plus']],
        ['a10x', '1668x2224', ['iPad Pro (2th gen 10.5-inch)']],
        ['a10x', '2048x2732', ['iPad Pro (2th gen 12.9-inch)']],
        ['a11', '750x1334', ['iPhone 8']],
        ['a11', '1242x2208', ['iPhone 8 Plus']],
        ['a11', '1125x2436', ['iPhone X']],
        ['a12', '828x1792', ['iPhone Xr']],
        ['a12', '1125x2436', ['iPhone Xs']],
        ['a12', '1242x2688', ['iPhone Xs Max']],
        ['a12x', '1668x2388', ['iPad Pro (3rd gen 11-inch)']],
        ['a12x', '2048x2732', ['iPad Pro (3rd gen 12.9-inch)']],
        ['a15', '2556x1179', ['iPhone 14']],
        ['a15', '2796x1290', ['iPhone 14 Max']]
    ];

function getCanvas() {
    if (canvas == null) {
        canvas = document.createElement('canvas');
    }

    return canvas;
}

function getGl() {
    if (gl == null) {
        gl = getCanvas().getContext('webgl');
    }

    return gl;
}

function getResolution() {
    var ratio = window.devicePixelRatio || 1;
    return (Math.min(screen.width, screen.height) * ratio)
        + 'x' + (Math.max(screen.width, screen.height) * ratio);
}

function getGlRenderer() {
    var glVendor = 'Unknown';

    if (glRenderer == null) {
        const gl = document.createElement("canvas").getContext("webgl");
        // try to get the extensions
        const ext = gl.getExtension("WEBGL_debug_renderer_info");

        // if the extension exists, find out the info.
        if (ext) {
            glRenderer = gl.getParameter(ext.UNMASKED_RENDERER_WEBGL);
            glVendor = gl.getParameter(ext.UNMASKED_VENDOR_WEBGL);
        }
        else {
            glRenderer = "Unknown";
        }
    }

    return { glRenderer, glVendor };
}

function getModels() {
    if (models === undefined) {
        var gpu = getGlRenderer();
        var matches = gpu.glRenderer.toLowerCase().includes('apple');
        var res = getResolution();

        models = ['unknown'];

        if (matches) {
            for (var i = 0; i < devices.length; i++) {
                var device = devices[i];

                var res2 = res.split('x').reverse().join('x');
                if (res == device[1] || res2 == device[1]) {
                    models = device[2];
                    break;
                }
            }
        }
    }

    return models;
}

function getDeviceInfo() {
    if (window.MobileDevice == undefined) {
        window.MobileDevice = {};
    }

    window.MobileDevice.getGlRenderer = getGlRenderer;
    window.MobileDevice.getModels = getModels;
    window.MobileDevice.getResolution = getResolution;

    var currentModels = getModels();
    //var match = match.toLowerCase().replace(/\s+$/, '') + ' ';

    for (var i = 0; i < currentModels.length; i++) {
        var model = currentModels[i].toLowerCase() + ' ';

        if (0 === model.indexOf(math)) {
            return true;
        }
    }
}

(function (window, undefined) {
    'use strict';

    //////////////
    // Constants
    /////////////

    var LIBVERSION = '0.7.31',
        EMPTY = '',
        UNKNOWN = '?',
        FUNC_TYPE = 'function',
        UNDEF_TYPE = 'undefined',
        OBJ_TYPE = 'object',
        STR_TYPE = 'string',
        MAJOR = 'major',
        MODEL = 'model',
        NAME = 'name',
        TYPE = 'type',
        VENDOR = 'vendor',
        VERSION = 'version',
        ARCHITECTURE = 'architecture',
        CONSOLE = 'console',
        MOBILE = 'mobile',
        TABLET = 'tablet',
        SMARTTV = 'smarttv',
        WEARABLE = 'wearable',
        EMBEDDED = 'embedded',
        UA_MAX_LENGTH = 275;

    var AMAZON = 'Amazon',
        APPLE = 'Apple',
        ASUS = 'ASUS',
        BLACKBERRY = 'BlackBerry',
        BROWSER = 'Browser',
        CHROME = 'Chrome',
        EDGE = 'Edge',
        FIREFOX = 'Firefox',
        GOOGLE = 'Google',
        HUAWEI = 'Huawei',
        LG = 'LG',
        MICROSOFT = 'Microsoft',
        MOTOROLA = 'Motorola',
        OPERA = 'Opera',
        SAMSUNG = 'Samsung',
        SONY = 'Sony',
        XIAOMI = 'Xiaomi',
        ZEBRA = 'Zebra',
        FACEBOOK = 'Facebook';

    ///////////
    // Helper
    //////////

    var extend = function (regexes, extensions) {
        var mergedRegexes = {};
        for (var i in regexes) {
            if (extensions[i] && extensions[i].length % 2 === 0) {
                mergedRegexes[i] = extensions[i].concat(regexes[i]);
            } else {
                mergedRegexes[i] = regexes[i];
            }
        }
        return mergedRegexes;
    },
        enumerize = function (arr) {
            var enums = {};
            for (var i = 0; i < arr.length; i++) {
                enums[arr[i].toUpperCase()] = arr[i];
            }
            return enums;
        },
        has = function (str1, str2) {
            return typeof str1 === STR_TYPE ? lowerize(str2).indexOf(lowerize(str1)) !== -1 : false;
        },
        lowerize = function (str) {
            return str.toLowerCase();
        },
        majorize = function (version) {
            return typeof (version) === STR_TYPE ? version.replace(/[^\d\.]/g, EMPTY).split('.')[0] : undefined;
        },
        trim = function (str, len) {
            if (typeof (str) === STR_TYPE) {
                str = str.replace(/^\s\s*/, EMPTY).replace(/\s\s*$/, EMPTY);
                return typeof (len) === UNDEF_TYPE ? str : str.substring(0, UA_MAX_LENGTH);
            }
        };

    ///////////////
    // Map helper
    //////////////

    var rgxMapper = function (ua, arrays) {

        var i = 0, j, k, p, q, matches, match;

        // loop through all regexes maps
        while (i < arrays.length && !matches) {

            var regex = arrays[i],       // even sequence (0,2,4,..)
                props = arrays[i + 1];   // odd sequence (1,3,5,..)
            j = k = 0;

            // try matching uastring with regexes
            while (j < regex.length && !matches) {

                matches = regex[j++].exec(ua);

                if (!!matches) {
                    for (p = 0; p < props.length; p++) {
                        match = matches[++k];
                        q = props[p];
                        // check if given property is actually array
                        if (typeof q === OBJ_TYPE && q.length > 0) {
                            if (q.length === 2) {
                                if (typeof q[1] == FUNC_TYPE) {
                                    // assign modified match
                                    this[q[0]] = q[1].call(this, match);
                                } else {
                                    // assign given value, ignore regex match
                                    this[q[0]] = q[1];
                                }
                            } else if (q.length === 3) {
                                // check whether function or regex
                                if (typeof q[1] === FUNC_TYPE && !(q[1].exec && q[1].test)) {
                                    // call function (usually string mapper)
                                    this[q[0]] = match ? q[1].call(this, match, q[2]) : undefined;
                                } else {
                                    // sanitize match using given regex
                                    this[q[0]] = match ? match.replace(q[1], q[2]) : undefined;
                                }
                            } else if (q.length === 4) {
                                this[q[0]] = match ? q[3].call(this, match.replace(q[1], q[2])) : undefined;
                            }
                        } else {
                            this[q] = match ? match : undefined;
                        }
                    }
                }
            }
            i += 2;
        }
    },

        strMapper = function (str, map) {

            for (var i in map) {
                // check if current value is array
                if (typeof map[i] === OBJ_TYPE && map[i].length > 0) {
                    for (var j = 0; j < map[i].length; j++) {
                        if (has(map[i][j], str)) {
                            return (i === UNKNOWN) ? undefined : i;
                        }
                    }
                } else if (has(map[i], str)) {
                    return (i === UNKNOWN) ? undefined : i;
                }
            }
            return str;
        };

    ///////////////
    // String map
    //////////////

    // Safari < 3.0
    var oldSafariMap = {
        '1.0': '/8',
        '1.2': '/1',
        '1.3': '/3',
        '2.0': '/412',
        '2.0.2': '/416',
        '2.0.3': '/417',
        '2.0.4': '/419',
        '?': '/'
    },
        windowsVersionMap = {
            'ME': '4.90',
            'NT 3.11': 'NT3.51',
            'NT 4.0': 'NT4.0',
            '2000': 'NT 5.0',
            'XP': ['NT 5.1', 'NT 5.2'],
            'Vista': 'NT 6.0',
            '7': 'NT 6.1',
            '8': 'NT 6.2',
            '8.1': 'NT 6.3',
            '10': ['NT 6.4', 'NT 10.0'],
            'RT': 'ARM'
        };

    //////////////
    // Regex map
    /////////////

    var regexes = {

        browser: [[

            /\b(?:crmo|crios)\/([\w\.]+)/i                                      // Chrome for Android/iOS
        ], [VERSION, [NAME, 'Chrome']], [
            /edg(?:e|ios|a)?\/([\w\.]+)/i                                       // Microsoft Edge
        ], [VERSION, [NAME, 'Edge']], [

            // Presto based
            /(opera mini)\/([-\w\.]+)/i,                                        // Opera Mini
            /(opera [mobiletab]{3,6})\b.+version\/([-\w\.]+)/i,                 // Opera Mobi/Tablet
            /(opera)(?:.+version\/|[\/ ]+)([\w\.]+)/i                           // Opera
        ], [NAME, VERSION], [
            /opios[\/ ]+([\w\.]+)/i                                             // Opera mini on iphone >= 8.0
        ], [VERSION, [NAME, OPERA + ' Mini']], [
            /\bopr\/([\w\.]+)/i                                                 // Opera Webkit
        ], [VERSION, [NAME, OPERA]], [

            // Mixed
            /(kindle)\/([\w\.]+)/i,                                             // Kindle
            /(lunascape|maxthon|netfront|jasmine|blazer)[\/ ]?([\w\.]*)/i,      // Lunascape/Maxthon/Netfront/Jasmine/Blazer
            // Trident based
            /(avant |iemobile|slim)(?:browser)?[\/ ]?([\w\.]*)/i,               // Avant/IEMobile/SlimBrowser
            /(ba?idubrowser)[\/ ]?([\w\.]+)/i,                                  // Baidu Browser
            /(?:ms|\()(ie) ([\w\.]+)/i,                                         // Internet Explorer

            // Webkit/KHTML based                                               // Flock/RockMelt/Midori/Epiphany/Silk/Skyfire/Bolt/Iron/Iridium/PhantomJS/Bowser/QupZilla/Falkon
            /(flock|rockmelt|midori|epiphany|silk|skyfire|ovibrowser|bolt|iron|vivaldi|iridium|phantomjs|bowser|quark|qupzilla|falkon|rekonq|puffin|brave|whale|qqbrowserlite|qq)\/([-\w\.]+)/i,
            // Rekonq/Puffin/Brave/Whale/QQBrowserLite/QQ, aka ShouQ
            /(weibo)__([\d\.]+)/i                                               // Weibo
        ], [NAME, VERSION], [
            /(?:\buc? ?browser|(?:juc.+)ucweb)[\/ ]?([\w\.]+)/i                 // UCBrowser
        ], [VERSION, [NAME, 'UC' + BROWSER]], [
            /\bqbcore\/([\w\.]+)/i                                              // WeChat Desktop for Windows Built-in Browser
        ], [VERSION, [NAME, 'WeChat(Win) Desktop']], [
            /micromessenger\/([\w\.]+)/i                                        // WeChat
        ], [VERSION, [NAME, 'WeChat']], [
            /konqueror\/([\w\.]+)/i                                             // Konqueror
        ], [VERSION, [NAME, 'Konqueror']], [
            /trident.+rv[: ]([\w\.]{1,9})\b.+like gecko/i                       // IE11
        ], [VERSION, [NAME, 'IE']], [
            /yabrowser\/([\w\.]+)/i                                             // Yandex
        ], [VERSION, [NAME, 'Yandex']], [
            /(avast|avg)\/([\w\.]+)/i                                           // Avast/AVG Secure Browser
        ], [[NAME, /(.+)/, '$1 Secure ' + BROWSER], VERSION], [
            /\bfocus\/([\w\.]+)/i                                               // Firefox Focus
        ], [VERSION, [NAME, FIREFOX + ' Focus']], [
            /\bopt\/([\w\.]+)/i                                                 // Opera Touch
        ], [VERSION, [NAME, OPERA + ' Touch']], [
            /coc_coc\w+\/([\w\.]+)/i                                            // Coc Coc Browser
        ], [VERSION, [NAME, 'Coc Coc']], [
            /dolfin\/([\w\.]+)/i                                                // Dolphin
        ], [VERSION, [NAME, 'Dolphin']], [
            /coast\/([\w\.]+)/i                                                 // Opera Coast
        ], [VERSION, [NAME, OPERA + ' Coast']], [
            /miuibrowser\/([\w\.]+)/i                                           // MIUI Browser
        ], [VERSION, [NAME, 'MIUI ' + BROWSER]], [
            /fxios\/([-\w\.]+)/i                                                // Firefox for iOS
        ], [VERSION, [NAME, FIREFOX]], [
            /\bqihu|(qi?ho?o?|360)browser/i                                     // 360
        ], [[NAME, '360 ' + BROWSER]], [
            /(oculus|samsung|sailfish)browser\/([\w\.]+)/i
        ], [[NAME, /(.+)/, '$1 ' + BROWSER], VERSION], [                      // Oculus/Samsung/Sailfish Browser
            /(comodo_dragon)\/([\w\.]+)/i                                       // Comodo Dragon
        ], [[NAME, /_/g, ' '], VERSION], [
            /(electron)\/([\w\.]+) safari/i,                                    // Electron-based App
            /(tesla)(?: qtcarbrowser|\/(20\d\d\.[-\w\.]+))/i,                   // Tesla
            /m?(qqbrowser|baiduboxapp|2345Explorer)[\/ ]?([\w\.]+)/i            // QQBrowser/Baidu App/2345 Browser
        ], [NAME, VERSION], [
            /(metasr)[\/ ]?([\w\.]+)/i,                                         // SouGouBrowser
            /(lbbrowser)/i                                                      // LieBao Browser
        ], [NAME], [

            // WebView
            /((?:fban\/fbios|fb_iab\/fb4a)(?!.+fbav)|;fbav\/([\w\.]+);)/i       // Facebook App for iOS & Android
        ], [[NAME, FACEBOOK], VERSION], [
            /safari (line)\/([\w\.]+)/i,                                        // Line App for iOS
            /\b(line)\/([\w\.]+)\/iab/i,                                        // Line App for Android
            /(chromium|instagram)[\/ ]([-\w\.]+)/i                              // Chromium/Instagram
        ], [NAME, VERSION], [
            /\bgsa\/([\w\.]+) .*safari\//i                                      // Google Search Appliance on iOS
        ], [VERSION, [NAME, 'GSA']], [

            /headlesschrome(?:\/([\w\.]+)| )/i                                  // Chrome Headless
        ], [VERSION, [NAME, CHROME + ' Headless']], [

            / wv\).+(chrome)\/([\w\.]+)/i                                       // Chrome WebView
        ], [[NAME, CHROME + ' WebView'], VERSION], [

            /droid.+ version\/([\w\.]+)\b.+(?:mobile safari|safari)/i           // Android Browser
        ], [VERSION, [NAME, 'Android ' + BROWSER]], [

            /(chrome|omniweb|arora|[tizenoka]{5} ?browser)\/v?([\w\.]+)/i       // Chrome/OmniWeb/Arora/Tizen/Nokia
        ], [NAME, VERSION], [

            /version\/([\w\.]+) .*mobile\/\w+ (safari)/i                        // Mobile Safari
        ], [VERSION, [NAME, 'Mobile Safari']], [
            /version\/([\w\.]+) .*(mobile ?safari|safari)/i                     // Safari & Safari Mobile
        ], [VERSION, NAME], [
            /webkit.+?(mobile ?safari|safari)(\/[\w\.]+)/i                      // Safari < 3.0
        ], [NAME, [VERSION, strMapper, oldSafariMap]], [

            /(webkit|khtml)\/([\w\.]+)/i
        ], [NAME, VERSION], [

            // Gecko based
            /(navigator|netscape\d?)\/([-\w\.]+)/i                              // Netscape
        ], [[NAME, 'Netscape'], VERSION], [
            /mobile vr; rv:([\w\.]+)\).+firefox/i                               // Firefox Reality
        ], [VERSION, [NAME, FIREFOX + ' Reality']], [
            /ekiohf.+(flow)\/([\w\.]+)/i,                                       // Flow
            /(swiftfox)/i,                                                      // Swiftfox
            /(icedragon|iceweasel|camino|chimera|fennec|maemo browser|minimo|conkeror|klar)[\/ ]?([\w\.\+]+)/i,
            // IceDragon/Iceweasel/Camino/Chimera/Fennec/Maemo/Minimo/Conkeror/Klar
            /(seamonkey|k-meleon|icecat|iceape|firebird|phoenix|palemoon|basilisk|waterfox)\/([-\w\.]+)$/i,
            // Firefox/SeaMonkey/K-Meleon/IceCat/IceApe/Firebird/Phoenix
            /(firefox)\/([\w\.]+)/i,                                            // Other Firefox-based
            /(mozilla)\/([\w\.]+) .+rv\:.+gecko\/\d+/i,                         // Mozilla

            // Other
            /(polaris|lynx|dillo|icab|doris|amaya|w3m|netsurf|sleipnir|obigo|mosaic|(?:go|ice|up)[\. ]?browser)[-\/ ]?v?([\w\.]+)/i,
            // Polaris/Lynx/Dillo/iCab/Doris/Amaya/w3m/NetSurf/Sleipnir/Obigo/Mosaic/Go/ICE/UP.Browser
            /(links) \(([\w\.]+)/i                                              // Links
        ], [NAME, VERSION]
        ],

        cpu: [[

            /(?:(amd|x(?:(?:86|64)[-_])?|wow|win)64)[;\)]/i                     // AMD64 (x64)
        ], [[ARCHITECTURE, 'amd64']], [

            /(ia32(?=;))/i                                                      // IA32 (quicktime)
        ], [[ARCHITECTURE, lowerize]], [

            /((?:i[346]|x)86)[;\)]/i                                            // IA32 (x86)
        ], [[ARCHITECTURE, 'ia32']], [

            /\b(aarch64|arm(v?8e?l?|_?64))\b/i                                 // ARM64
        ], [[ARCHITECTURE, 'arm64']], [

            /\b(arm(?:v[67])?ht?n?[fl]p?)\b/i                                   // ARMHF
        ], [[ARCHITECTURE, 'armhf']], [

            // PocketPC mistakenly identified as PowerPC
            /windows (ce|mobile); ppc;/i
        ], [[ARCHITECTURE, 'arm']], [

            /((?:ppc|powerpc)(?:64)?)(?: mac|;|\))/i                            // PowerPC
        ], [[ARCHITECTURE, /ower/, EMPTY, lowerize]], [

            /(sun4\w)[;\)]/i                                                    // SPARC
        ], [[ARCHITECTURE, 'sparc']], [

            /((?:avr32|ia64(?=;))|68k(?=\))|\barm(?=v(?:[1-7]|[5-7]1)l?|;|eabi)|(?=atmel )avr|(?:irix|mips|sparc)(?:64)?\b|pa-risc)/i
            // IA64, 68K, ARM/64, AVR/32, IRIX/64, MIPS/64, SPARC/64, PA-RISC
        ], [[ARCHITECTURE, lowerize]]
        ],

        device: [[

            //////////////////////////
            // MOBILES & TABLETS
            // Ordered by popularity
            /////////////////////////

            // Samsung
            /\b(sch-i[89]0\d|shw-m380s|sm-[pt]\w{2,4}|gt-[pn]\d{2,4}|sgh-t8[56]9|nexus 10)/i
        ], [MODEL, [VENDOR, SAMSUNG], [TYPE, TABLET]], [
            /\b((?:s[cgp]h|gt|sm)-\w+|galaxy nexus)/i,
            /samsung[- ]([-\w]+)/i,
            /sec-(sgh\w+)/i
        ], [MODEL, [VENDOR, SAMSUNG], [TYPE, MOBILE]], [

            // Apple
            /\((ip(?:hone|od)[\w ]*);/i                                         // iPod/iPhone
        ], [MODEL, [VENDOR, APPLE], [TYPE, MOBILE]], [
            /\((ipad);[-\w\),; ]+apple/i,                                       // iPad
            /applecoremedia\/[\w\.]+ \((ipad)/i,
            /\b(ipad)\d\d?,\d\d?[;\]].+ios/i
        ], [MODEL, [VENDOR, APPLE], [TYPE, TABLET]], [

            // Huawei
            /\b((?:ag[rs][23]?|bah2?|sht?|btv)-a?[lw]\d{2})\b(?!.+d\/s)/i
        ], [MODEL, [VENDOR, HUAWEI], [TYPE, TABLET]], [
            /(?:huawei|honor)([-\w ]+)[;\)]/i,
            /\b(nexus 6p|\w{2,4}-[atu]?[ln][01259x][012359][an]?)\b(?!.+d\/s)/i
        ], [MODEL, [VENDOR, HUAWEI], [TYPE, MOBILE]], [

            // Xiaomi
            /\b(poco[\w ]+)(?: bui|\))/i,                                       // Xiaomi POCO
            /\b; (\w+) build\/hm\1/i,                                           // Xiaomi Hongmi 'numeric' models
            /\b(hm[-_ ]?note?[_ ]?(?:\d\w)?) bui/i,                             // Xiaomi Hongmi
            /\b(redmi[\-_ ]?(?:note|k)?[\w_ ]+)(?: bui|\))/i,                   // Xiaomi Redmi
            /\b(mi[-_ ]?(?:a\d|one|one[_ ]plus|note lte|max|cc)?[_ ]?(?:\d?\w?)[_ ]?(?:plus|se|lite)?)(?: bui|\))/i // Xiaomi Mi
        ], [[MODEL, /_/g, ' '], [VENDOR, XIAOMI], [TYPE, MOBILE]], [
            /\b(mi[-_ ]?(?:pad)(?:[\w_ ]+))(?: bui|\))/i                        // Mi Pad tablets
        ], [[MODEL, /_/g, ' '], [VENDOR, XIAOMI], [TYPE, TABLET]], [

            // OPPO
            /; (\w+) bui.+ oppo/i,
            /\b(cph[12]\d{3}|p(?:af|c[al]|d\w|e[ar])[mt]\d0|x9007|a101op)\b/i
        ], [MODEL, [VENDOR, 'OPPO'], [TYPE, MOBILE]], [

            // Vivo
            /vivo (\w+)(?: bui|\))/i,
            /\b(v[12]\d{3}\w?[at])(?: bui|;)/i
        ], [MODEL, [VENDOR, 'Vivo'], [TYPE, MOBILE]], [

            // Realme
            /\b(rmx[12]\d{3})(?: bui|;|\))/i
        ], [MODEL, [VENDOR, 'Realme'], [TYPE, MOBILE]], [

            // Motorola
            /\b(milestone|droid(?:[2-4x]| (?:bionic|x2|pro|razr))?:?( 4g)?)\b[\w ]+build\//i,
            /\bmot(?:orola)?[- ](\w*)/i,
            /((?:moto[\w\(\) ]+|xt\d{3,4}|nexus 6)(?= bui|\)))/i
        ], [MODEL, [VENDOR, MOTOROLA], [TYPE, MOBILE]], [
            /\b(mz60\d|xoom[2 ]{0,2}) build\//i
        ], [MODEL, [VENDOR, MOTOROLA], [TYPE, TABLET]], [

            // LG
            /((?=lg)?[vl]k\-?\d{3}) bui| 3\.[-\w; ]{10}lg?-([06cv9]{3,4})/i
        ], [MODEL, [VENDOR, LG], [TYPE, TABLET]], [
            /(lm(?:-?f100[nv]?|-[\w\.]+)(?= bui|\))|nexus [45])/i,
            /\blg[-e;\/ ]+((?!browser|netcast|android tv)\w+)/i,
            /\blg-?([\d\w]+) bui/i
        ], [MODEL, [VENDOR, LG], [TYPE, MOBILE]], [

            // Lenovo
            /(ideatab[-\w ]+)/i,
            /lenovo ?(s[56]000[-\w]+|tab(?:[\w ]+)|yt[-\d\w]{6}|tb[-\d\w]{6})/i
        ], [MODEL, [VENDOR, 'Lenovo'], [TYPE, TABLET]], [

            // Nokia
            /(?:maemo|nokia).*(n900|lumia \d+)/i,
            /nokia[-_ ]?([-\w\.]*)/i
        ], [[MODEL, /_/g, ' '], [VENDOR, 'Nokia'], [TYPE, MOBILE]], [

            // Google
            /(pixel c)\b/i                                                      // Google Pixel C
        ], [MODEL, [VENDOR, GOOGLE], [TYPE, TABLET]], [
            /droid.+; (pixel[\daxl ]{0,6})(?: bui|\))/i                         // Google Pixel
        ], [MODEL, [VENDOR, GOOGLE], [TYPE, MOBILE]], [

            // Sony
            /droid.+ (a?\d[0-2]{2}so|[c-g]\d{4}|so[-gl]\w+|xq-a\w[4-7][12])(?= bui|\).+chrome\/(?![1-6]{0,1}\d\.))/i
        ], [MODEL, [VENDOR, SONY], [TYPE, MOBILE]], [
            /sony tablet [ps]/i,
            /\b(?:sony)?sgp\w+(?: bui|\))/i
        ], [[MODEL, 'Xperia Tablet'], [VENDOR, SONY], [TYPE, TABLET]], [

            // OnePlus
            / (kb2005|in20[12]5|be20[12][59])\b/i,
            /(?:one)?(?:plus)? (a\d0\d\d)(?: b|\))/i
        ], [MODEL, [VENDOR, 'OnePlus'], [TYPE, MOBILE]], [

            // Amazon
            /(alexa)webm/i,
            /(kf[a-z]{2}wi)( bui|\))/i,                                         // Kindle Fire without Silk
            /(kf[a-z]+)( bui|\)).+silk\//i                                      // Kindle Fire HD
        ], [MODEL, [VENDOR, AMAZON], [TYPE, TABLET]], [
            /((?:sd|kf)[0349hijorstuw]+)( bui|\)).+silk\//i                     // Fire Phone
        ], [[MODEL, /(.+)/g, 'Fire Phone $1'], [VENDOR, AMAZON], [TYPE, MOBILE]], [

            // BlackBerry
            /(playbook);[-\w\),; ]+(rim)/i                                      // BlackBerry PlayBook
        ], [MODEL, VENDOR, [TYPE, TABLET]], [
            /\b((?:bb[a-f]|st[hv])100-\d)/i,
            /\(bb10; (\w+)/i                                                    // BlackBerry 10
        ], [MODEL, [VENDOR, BLACKBERRY], [TYPE, MOBILE]], [

            // Asus
            /(?:\b|asus_)(transfo[prime ]{4,10} \w+|eeepc|slider \w+|nexus 7|padfone|p00[cj])/i
        ], [MODEL, [VENDOR, ASUS], [TYPE, TABLET]], [
            / (z[bes]6[027][012][km][ls]|zenfone \d\w?)\b/i
        ], [MODEL, [VENDOR, ASUS], [TYPE, MOBILE]], [

            // HTC
            /(nexus 9)/i                                                        // HTC Nexus 9
        ], [MODEL, [VENDOR, 'HTC'], [TYPE, TABLET]], [
            /(htc)[-;_ ]{1,2}([\w ]+(?=\)| bui)|\w+)/i,                         // HTC

            // ZTE
            /(zte)[- ]([\w ]+?)(?: bui|\/|\))/i,
            /(alcatel|geeksphone|nexian|panasonic|sony(?!-bra))[-_ ]?([-\w]*)/i         // Alcatel/GeeksPhone/Nexian/Panasonic/Sony
        ], [VENDOR, [MODEL, /_/g, ' '], [TYPE, MOBILE]], [

            // Acer
            /droid.+; ([ab][1-7]-?[0178a]\d\d?)/i
        ], [MODEL, [VENDOR, 'Acer'], [TYPE, TABLET]], [

            // Meizu
            /droid.+; (m[1-5] note) bui/i,
            /\bmz-([-\w]{2,})/i
        ], [MODEL, [VENDOR, 'Meizu'], [TYPE, MOBILE]], [

            // Sharp
            /\b(sh-?[altvz]?\d\d[a-ekm]?)/i
        ], [MODEL, [VENDOR, 'Sharp'], [TYPE, MOBILE]], [

            // MIXED
            /(blackberry|benq|palm(?=\-)|sonyericsson|acer|asus|dell|meizu|motorola|polytron)[-_ ]?([-\w]*)/i,
            // BlackBerry/BenQ/Palm/Sony-Ericsson/Acer/Asus/Dell/Meizu/Motorola/Polytron
            /(hp) ([\w ]+\w)/i,                                                 // HP iPAQ
            /(asus)-?(\w+)/i,                                                   // Asus
            /(microsoft); (lumia[\w ]+)/i,                                      // Microsoft Lumia
            /(lenovo)[-_ ]?([-\w]+)/i,                                          // Lenovo
            /(jolla)/i,                                                         // Jolla
            /(oppo) ?([\w ]+) bui/i                                             // OPPO
        ], [VENDOR, MODEL, [TYPE, MOBILE]], [

            /(archos) (gamepad2?)/i,                                            // Archos
            /(hp).+(touchpad(?!.+tablet)|tablet)/i,                             // HP TouchPad
            /(kindle)\/([\w\.]+)/i,                                             // Kindle
            /(nook)[\w ]+build\/(\w+)/i,                                        // Nook
            /(dell) (strea[kpr\d ]*[\dko])/i,                                   // Dell Streak
            /(le[- ]+pan)[- ]+(\w{1,9}) bui/i,                                  // Le Pan Tablets
            /(trinity)[- ]*(t\d{3}) bui/i,                                      // Trinity Tablets
            /(gigaset)[- ]+(q\w{1,9}) bui/i,                                    // Gigaset Tablets
            /(vodafone) ([\w ]+)(?:\)| bui)/i                                   // Vodafone
        ], [VENDOR, MODEL, [TYPE, TABLET]], [

            /(surface duo)/i                                                    // Surface Duo
        ], [MODEL, [VENDOR, MICROSOFT], [TYPE, TABLET]], [
            /droid [\d\.]+; (fp\du?)(?: b|\))/i                                 // Fairphone
        ], [MODEL, [VENDOR, 'Fairphone'], [TYPE, MOBILE]], [
            /(u304aa)/i                                                         // AT&T
        ], [MODEL, [VENDOR, 'AT&T'], [TYPE, MOBILE]], [
            /\bsie-(\w*)/i                                                      // Siemens
        ], [MODEL, [VENDOR, 'Siemens'], [TYPE, MOBILE]], [
            /\b(rct\w+) b/i                                                     // RCA Tablets
        ], [MODEL, [VENDOR, 'RCA'], [TYPE, TABLET]], [
            /\b(venue[\d ]{2,7}) b/i                                            // Dell Venue Tablets
        ], [MODEL, [VENDOR, 'Dell'], [TYPE, TABLET]], [
            /\b(q(?:mv|ta)\w+) b/i                                              // Verizon Tablet
        ], [MODEL, [VENDOR, 'Verizon'], [TYPE, TABLET]], [
            /\b(?:barnes[& ]+noble |bn[rt])([\w\+ ]*) b/i                       // Barnes & Noble Tablet
        ], [MODEL, [VENDOR, 'Barnes & Noble'], [TYPE, TABLET]], [
            /\b(tm\d{3}\w+) b/i
        ], [MODEL, [VENDOR, 'NuVision'], [TYPE, TABLET]], [
            /\b(k88) b/i                                                        // ZTE K Series Tablet
        ], [MODEL, [VENDOR, 'ZTE'], [TYPE, TABLET]], [
            /\b(nx\d{3}j) b/i                                                   // ZTE Nubia
        ], [MODEL, [VENDOR, 'ZTE'], [TYPE, MOBILE]], [
            /\b(gen\d{3}) b.+49h/i                                              // Swiss GEN Mobile
        ], [MODEL, [VENDOR, 'Swiss'], [TYPE, MOBILE]], [
            /\b(zur\d{3}) b/i                                                   // Swiss ZUR Tablet
        ], [MODEL, [VENDOR, 'Swiss'], [TYPE, TABLET]], [
            /\b((zeki)?tb.*\b) b/i                                              // Zeki Tablets
        ], [MODEL, [VENDOR, 'Zeki'], [TYPE, TABLET]], [
            /\b([yr]\d{2}) b/i,
            /\b(dragon[- ]+touch |dt)(\w{5}) b/i                                // Dragon Touch Tablet
        ], [[VENDOR, 'Dragon Touch'], MODEL, [TYPE, TABLET]], [
            /\b(ns-?\w{0,9}) b/i                                                // Insignia Tablets
        ], [MODEL, [VENDOR, 'Insignia'], [TYPE, TABLET]], [
            /\b((nxa|next)-?\w{0,9}) b/i                                        // NextBook Tablets
        ], [MODEL, [VENDOR, 'NextBook'], [TYPE, TABLET]], [
            /\b(xtreme\_)?(v(1[045]|2[015]|[3469]0|7[05])) b/i                  // Voice Xtreme Phones
        ], [[VENDOR, 'Voice'], MODEL, [TYPE, MOBILE]], [
            /\b(lvtel\-)?(v1[12]) b/i                                           // LvTel Phones
        ], [[VENDOR, 'LvTel'], MODEL, [TYPE, MOBILE]], [
            /\b(ph-1) /i                                                        // Essential PH-1
        ], [MODEL, [VENDOR, 'Essential'], [TYPE, MOBILE]], [
            /\b(v(100md|700na|7011|917g).*\b) b/i                               // Envizen Tablets
        ], [MODEL, [VENDOR, 'Envizen'], [TYPE, TABLET]], [
            /\b(trio[-\w\. ]+) b/i                                              // MachSpeed Tablets
        ], [MODEL, [VENDOR, 'MachSpeed'], [TYPE, TABLET]], [
            /\btu_(1491) b/i                                                    // Rotor Tablets
        ], [MODEL, [VENDOR, 'Rotor'], [TYPE, TABLET]], [
            /(shield[\w ]+) b/i                                                 // Nvidia Shield Tablets
        ], [MODEL, [VENDOR, 'Nvidia'], [TYPE, TABLET]], [
            /(sprint) (\w+)/i                                                   // Sprint Phones
        ], [VENDOR, MODEL, [TYPE, MOBILE]], [
            /(kin\.[onetw]{3})/i                                                // Microsoft Kin
        ], [[MODEL, /\./g, ' '], [VENDOR, MICROSOFT], [TYPE, MOBILE]], [
            /droid.+; (cc6666?|et5[16]|mc[239][23]x?|vc8[03]x?)\)/i             // Zebra
        ], [MODEL, [VENDOR, ZEBRA], [TYPE, TABLET]], [
            /droid.+; (ec30|ps20|tc[2-8]\d[kx])\)/i
        ], [MODEL, [VENDOR, ZEBRA], [TYPE, MOBILE]], [

            ///////////////////
            // CONSOLES
            ///////////////////

            /(ouya)/i,                                                          // Ouya
            /(nintendo) ([wids3utch]+)/i                                        // Nintendo
        ], [VENDOR, MODEL, [TYPE, CONSOLE]], [
            /droid.+; (shield) bui/i                                            // Nvidia
        ], [MODEL, [VENDOR, 'Nvidia'], [TYPE, CONSOLE]], [
            /(playstation [345portablevi]+)/i                                   // Playstation
        ], [MODEL, [VENDOR, SONY], [TYPE, CONSOLE]], [
            /\b(xbox(?: one)?(?!; xbox))[\); ]/i                                // Microsoft Xbox
        ], [MODEL, [VENDOR, MICROSOFT], [TYPE, CONSOLE]], [

            ///////////////////
            // SMARTTVS
            ///////////////////

            /smart-tv.+(samsung)/i                                              // Samsung
        ], [VENDOR, [TYPE, SMARTTV]], [
            /hbbtv.+maple;(\d+)/i
        ], [[MODEL, /^/, 'SmartTV'], [VENDOR, SAMSUNG], [TYPE, SMARTTV]], [
            /(nux; netcast.+smarttv|lg (netcast\.tv-201\d|android tv))/i        // LG SmartTV
        ], [[VENDOR, LG], [TYPE, SMARTTV]], [
            /(apple) ?tv/i                                                      // Apple TV
        ], [VENDOR, [MODEL, APPLE + ' TV'], [TYPE, SMARTTV]], [
            /crkey/i                                                            // Google Chromecast
        ], [[MODEL, CHROME + 'cast'], [VENDOR, GOOGLE], [TYPE, SMARTTV]], [
            /droid.+aft(\w)( bui|\))/i                                          // Fire TV
        ], [MODEL, [VENDOR, AMAZON], [TYPE, SMARTTV]], [
            /\(dtv[\);].+(aquos)/i                                              // Sharp
        ], [MODEL, [VENDOR, 'Sharp'], [TYPE, SMARTTV]], [
            /(bravia[\w- ]+) bui/i                                              // Sony
        ], [MODEL, [VENDOR, SONY], [TYPE, SMARTTV]], [
            /\b(roku)[\dx]*[\)\/]((?:dvp-)?[\d\.]*)/i,                          // Roku
            /hbbtv\/\d+\.\d+\.\d+ +\([\w ]*; *(\w[^;]*);([^;]*)/i               // HbbTV devices
        ], [[VENDOR, trim], [MODEL, trim], [TYPE, SMARTTV]], [
            /\b(android tv|smart[- ]?tv|opera tv|tv; rv:)\b/i                   // SmartTV from Unidentified Vendors
        ], [[TYPE, SMARTTV]], [

            ///////////////////
            // WEARABLES
            ///////////////////

            /((pebble))app/i                                                    // Pebble
        ], [VENDOR, MODEL, [TYPE, WEARABLE]], [
            /droid.+; (glass) \d/i                                              // Google Glass
        ], [MODEL, [VENDOR, GOOGLE], [TYPE, WEARABLE]], [
            /droid.+; (wt63?0{2,3})\)/i
        ], [MODEL, [VENDOR, ZEBRA], [TYPE, WEARABLE]], [
            /(quest( 2)?)/i                                                     // Oculus Quest
        ], [MODEL, [VENDOR, FACEBOOK], [TYPE, WEARABLE]], [

            ///////////////////
            // EMBEDDED
            ///////////////////

            /(tesla)(?: qtcarbrowser|\/[-\w\.]+)/i                              // Tesla
        ], [VENDOR, [TYPE, EMBEDDED]], [

            ////////////////////
            // MIXED (GENERIC)
            ///////////////////

            /droid .+?; ([^;]+?)(?: bui|\) applew).+? mobile safari/i           // Android Phones from Unidentified Vendors
        ], [MODEL, [TYPE, MOBILE]], [
            /droid .+?; ([^;]+?)(?: bui|\) applew).+?(?! mobile) safari/i       // Android Tablets from Unidentified Vendors
        ], [MODEL, [TYPE, TABLET]], [
            /\b((tablet|tab)[;\/]|focus\/\d(?!.+mobile))/i                      // Unidentifiable Tablet
        ], [[TYPE, TABLET]], [
            /(phone|mobile(?:[;\/]| safari)|pda(?=.+windows ce))/i              // Unidentifiable Mobile
        ], [[TYPE, MOBILE]], [
            /(android[-\w\. ]{0,9});.+buil/i                                    // Generic Android Device
        ], [MODEL, [VENDOR, 'Generic']]
        ],

        engine: [[

            /windows.+ edge\/([\w\.]+)/i                                       // EdgeHTML
        ], [VERSION, [NAME, EDGE + 'HTML']], [

            /webkit\/537\.36.+chrome\/(?!27)([\w\.]+)/i                         // Blink
        ], [VERSION, [NAME, 'Blink']], [

            /(presto)\/([\w\.]+)/i,                                             // Presto
            /(webkit|trident|netfront|netsurf|amaya|lynx|w3m|goanna)\/([\w\.]+)/i, // WebKit/Trident/NetFront/NetSurf/Amaya/Lynx/w3m/Goanna
            /ekioh(flow)\/([\w\.]+)/i,                                          // Flow
            /(khtml|tasman|links)[\/ ]\(?([\w\.]+)/i,                           // KHTML/Tasman/Links
            /(icab)[\/ ]([23]\.[\d\.]+)/i                                       // iCab
        ], [NAME, VERSION], [

            /rv\:([\w\.]{1,9})\b.+(gecko)/i                                     // Gecko
        ], [VERSION, NAME]
        ],

        os: [[

            // Windows
            /microsoft (windows) (vista|xp)/i                                   // Windows (iTunes)
        ], [NAME, VERSION], [
            /(windows) nt 6\.2; (arm)/i,                                        // Windows RT
            /(windows (?:phone(?: os)?|mobile))[\/ ]?([\d\.\w ]*)/i,            // Windows Phone
            /(windows)[\/ ]?([ntce\d\. ]+\w)(?!.+xbox)/i
        ], [NAME, [VERSION, strMapper, windowsVersionMap]], [
            /(win(?=3|9|n)|win 9x )([nt\d\.]+)/i
        ], [[NAME, 'Windows'], [VERSION, strMapper, windowsVersionMap]], [

            // iOS/macOS
            /ip[honead]{2,4}\b(?:.*os ([\w]+) like mac|; opera)/i,              // iOS
            /cfnetwork\/.+darwin/i
        ], [[VERSION, /_/g, '.'], [NAME, 'iOS']], [
            /(mac os x) ?([\w\. ]*)/i,
            /(macintosh|mac_powerpc\b)(?!.+haiku)/i                             // Mac OS
        ], [[NAME, 'Mac OS'], [VERSION, /_/g, '.']], [

            // Mobile OSes
            /droid ([\w\.]+)\b.+(android[- ]x86)/i                              // Android-x86
        ], [VERSION, NAME], [                                               // Android/WebOS/QNX/Bada/RIM/Maemo/MeeGo/Sailfish OS
            /(android|webos|qnx|bada|rim tablet os|maemo|meego|sailfish)[-\/ ]?([\w\.]*)/i,
            /(blackberry)\w*\/([\w\.]*)/i,                                      // Blackberry
            /(tizen|kaios)[\/ ]([\w\.]+)/i,                                     // Tizen/KaiOS
            /\((series40);/i                                                    // Series 40
        ], [NAME, VERSION], [
            /\(bb(10);/i                                                        // BlackBerry 10
        ], [VERSION, [NAME, BLACKBERRY]], [
            /(?:symbian ?os|symbos|s60(?=;)|series60)[-\/ ]?([\w\.]*)/i         // Symbian
        ], [VERSION, [NAME, 'Symbian']], [
            /mozilla\/[\d\.]+ \((?:mobile|tablet|tv|mobile; [\w ]+); rv:.+ gecko\/([\w\.]+)/i // Firefox OS
        ], [VERSION, [NAME, FIREFOX + ' OS']], [
            /web0s;.+rt(tv)/i,
            /\b(?:hp)?wos(?:browser)?\/([\w\.]+)/i                              // WebOS
        ], [VERSION, [NAME, 'webOS']], [

            // Google Chromecast
            /crkey\/([\d\.]+)/i                                                 // Google Chromecast
        ], [VERSION, [NAME, CHROME + 'cast']], [
            /(cros) [\w]+ ([\w\.]+\w)/i                                         // Chromium OS
        ], [[NAME, 'Chromium OS'], VERSION], [

            // Console
            /(nintendo|playstation) ([wids345portablevuch]+)/i,                 // Nintendo/Playstation
            /(xbox); +xbox ([^\);]+)/i,                                         // Microsoft Xbox (360, One, X, S, Series X, Series S)

            // Other
            /\b(joli|palm)\b ?(?:os)?\/?([\w\.]*)/i,                            // Joli/Palm
            /(mint)[\/\(\) ]?(\w*)/i,                                           // Mint
            /(mageia|vectorlinux)[; ]/i,                                        // Mageia/VectorLinux
            /([kxln]?ubuntu|debian|suse|opensuse|gentoo|arch(?= linux)|slackware|fedora|mandriva|centos|pclinuxos|red ?hat|zenwalk|linpus|raspbian|plan 9|minix|risc os|contiki|deepin|manjaro|elementary os|sabayon|linspire)(?: gnu\/linux)?(?: enterprise)?(?:[- ]linux)?(?:-gnu)?[-\/ ]?(?!chrom|package)([-\w\.]*)/i,
            // Ubuntu/Debian/SUSE/Gentoo/Arch/Slackware/Fedora/Mandriva/CentOS/PCLinuxOS/RedHat/Zenwalk/Linpus/Raspbian/Plan9/Minix/RISCOS/Contiki/Deepin/Manjaro/elementary/Sabayon/Linspire
            /(hurd|linux) ?([\w\.]*)/i,                                         // Hurd/Linux
            /(gnu) ?([\w\.]*)/i,                                                // GNU
            /\b([-frentopcghs]{0,5}bsd|dragonfly)[\/ ]?(?!amd|[ix346]{1,2}86)([\w\.]*)/i, // FreeBSD/NetBSD/OpenBSD/PC-BSD/GhostBSD/DragonFly
            /(haiku) (\w+)/i                                                    // Haiku
        ], [NAME, VERSION], [
            /(sunos) ?([\w\.\d]*)/i                                             // Solaris
        ], [[NAME, 'Solaris'], VERSION], [
            /((?:open)?solaris)[-\/ ]?([\w\.]*)/i,                              // Solaris
            /(aix) ((\d)(?=\.|\)| )[\w\.])*/i,                                  // AIX
            /\b(beos|os\/2|amigaos|morphos|openvms|fuchsia|hp-ux)/i,            // BeOS/OS2/AmigaOS/MorphOS/OpenVMS/Fuchsia/HP-UX
            /(unix) ?([\w\.]*)/i                                                // UNIX
        ], [NAME, VERSION]
        ]
    };

    /////////////////
    // Constructor
    ////////////////

    var UAParser = function (ua, extensions) {

        if (typeof ua === OBJ_TYPE) {
            extensions = ua;
            ua = undefined;
        }

        if (!(this instanceof UAParser)) {
            return new UAParser(ua, extensions).getResult();
        }

        var _ua = ua || ((typeof window !== UNDEF_TYPE && window.navigator && window.navigator.userAgent) ? window.navigator.userAgent : EMPTY);
        var _rgxmap = extensions ? extend(regexes, extensions) : regexes;

        this.getBrowser = function () {
            var _browser = {};
            _browser[NAME] = undefined;
            _browser[VERSION] = undefined;
            rgxMapper.call(_browser, _ua, _rgxmap.browser);
            _browser.major = majorize(_browser.version);
            return _browser;
        };
        this.getCPU = function () {
            var _cpu = {};
            _cpu[ARCHITECTURE] = undefined;
            rgxMapper.call(_cpu, _ua, _rgxmap.cpu);
            return _cpu;
        };
        this.getDevice = function () {
            var _device = {};
            _device[VENDOR] = undefined;
            _device[MODEL] = undefined;
            _device[TYPE] = undefined;
            rgxMapper.call(_device, _ua, _rgxmap.device);
            return _device;
        };
        this.getEngine = function () {
            var _engine = {};
            _engine[NAME] = undefined;
            _engine[VERSION] = undefined;
            rgxMapper.call(_engine, _ua, _rgxmap.engine);
            return _engine;
        };
        this.getOS = function () {
            var _os = {};
            _os[NAME] = undefined;
            _os[VERSION] = undefined;
            rgxMapper.call(_os, _ua, _rgxmap.os);
            return _os;
        };
        this.getResult = function () {
            return {
                ua: this.getUA(),
                browser: this.getBrowser(),
                engine: this.getEngine(),
                os: this.getOS(),
                device: this.getDevice(),
                cpu: this.getCPU()
            };
        };
        this.getUA = function () {
            return _ua;
        };
        this.setUA = function (ua) {
            _ua = (typeof ua === STR_TYPE && ua.length > UA_MAX_LENGTH) ? trim(ua, UA_MAX_LENGTH) : ua;
            return this;
        };
        this.setUA(_ua);
        return this;
    };

    UAParser.VERSION = LIBVERSION;
    UAParser.BROWSER = enumerize([NAME, VERSION, MAJOR]);
    UAParser.CPU = enumerize([ARCHITECTURE]);
    UAParser.DEVICE = enumerize([MODEL, VENDOR, TYPE, CONSOLE, MOBILE, SMARTTV, TABLET, WEARABLE, EMBEDDED]);
    UAParser.ENGINE = UAParser.OS = enumerize([NAME, VERSION]);

    ///////////
    // Export
    //////////

    // check js environment
    if (typeof (exports) !== UNDEF_TYPE) {
        // nodejs env
        if (typeof module !== UNDEF_TYPE && module.exports) {
            exports = module.exports = UAParser;
        }
        exports.UAParser = UAParser;
    } else {
        // requirejs env (optional)
        if (typeof (define) === FUNC_TYPE && define.amd) {
            define(function () {
                return UAParser;
            });
        } else if (typeof window !== UNDEF_TYPE) {
            // browser env
            window.UAParser = UAParser;
        }
    }

    // jQuery/Zepto specific (optional)
    // Note:
    //   In AMD env the global scope should be kept clean, but jQuery is an exception.
    //   jQuery always exports to global scope, unless jQuery.noConflict(true) is used,
    //   and we should catch that.
    var $ = typeof window !== UNDEF_TYPE && (window.jQuery || window.Zepto);
    if ($ && !$.ua) {
        var parser = new UAParser();
        $.ua = parser.getResult();
        $.ua.get = function () {
            return parser.getUA();
        };
        $.ua.set = function (ua) {
            parser.setUA(ua);
            var result = parser.getResult();
            for (var prop in result) {
                $.ua[prop] = result[prop];
            }
        };
    }

})(typeof window === 'object' ? window : this);