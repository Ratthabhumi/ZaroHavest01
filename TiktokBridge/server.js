// เรียกใช้ Library
const { WebcastPushConnection } = require('tiktok-live-connector');
const WebSocket = require('ws');

// 🔴🔴🔴 แก้ชื่อช่อง TikTok ของเฮียตรงนี้ (สำคัญ!) 🔴🔴🔴
let tiktokUsername = "mewmewnakub"; // ใส่ชื่อช่องเฮียที่นี่ (ไม่ต้องมี @)

// ========================================================

// 1. สร้าง WebSocket Server (เพื่อคุยกับ Unity)
const wss = new WebSocket.Server({ port: 8080 });

// เก็บรายการ Connection ของ Unity
wss.on('connection', function connection(ws) {
    console.log('✅ Unity Connected to Bridge!');
});

// ฟังก์ชันสำหรับส่งข้อมูลไปหา Unity
function sendToUnity(data) {
    wss.clients.forEach(function each(client) {
        if (client.readyState === WebSocket.OPEN) {
            client.send(JSON.stringify(data));
        }
    });
}

// 2. เชื่อมต่อกับ TikTok Live
let tiktokLiveConnection = new WebcastPushConnection(tiktokUsername);

// สั่ง Connect
tiktokLiveConnection.connect().then(state => {
    console.info(`🚀 Connected to TikTok RoomId: ${state.roomId}`);
}).catch(err => {
    console.error('❌ Failed to connect to TikTok:', err);
});

// ========================================================
// 🎯 โซนตั้งค่า Event (Like / Gift / Follow)
// ========================================================

// 1. เมื่อมีคนกดใจ (Like) -> สะสมให้ฝนตก
tiktokLiveConnection.on('like', (data) => {
    // data.likeCount คือจำนวนที่เขากดมาในชุดนี้
    console.log(`${data.uniqueId} กดใจมา x${data.likeCount}`);

    sendToUnity({
        type: 'like',
        name: data.nickname || data.uniqueId, // ส่งชื่อคนกด (เอาไว้ขึ้นโชว์ตอนฝนตก)
        count: data.likeCount
    });
});

// 2. เมื่อมีคนส่งของขวัญ (Gift) -> เสกต้นไม้
tiktokLiveConnection.on('gift', (data) => {
    // กรองเฉพาะกุหลาบ (Rose) หรือจะเอาหมดก็ได้ (Unity ไปเช็คต่อเอง)
    console.log(`${data.uniqueId} ส่งของขวัญ: ${data.giftName}`);

    if (data.giftType === 1 && !data.repeatEnd) {
        // ถ้าเป็นการส่งรัวๆ ให้รอจนจบชุดค่อยส่ง (Optional)
        // หรือจะส่งเลยก็ได้
    }

    sendToUnity({
        type: 'gift',
        name: data.nickname || data.uniqueId, // ส่งชื่อคนเปย์
        msg: data.giftName, // ส่งชื่อของขวัญ (เช่น Rose)
        count: 1
    });
});

// 3. เมื่อมีคนกดติดตาม (Follow) -> เสกพระอาทิตย์ (แทนการพิมพ์ Heart)
tiktokLiveConnection.on('follow', (data) => {
    console.log(`${data.uniqueId} เพิ่งกดติดตาม!`);

    sendToUnity({
        type: 'follow',
        name: data.nickname || data.uniqueId, // ส่งชื่อคนติดตาม
        count: 1
    });
});

// (แถม) แสดง Chat ใน Console ไว้ดูเล่น (แต่ไม่ได้ส่งไป Unity)
tiktokLiveConnection.on('chat', (data) => {
    console.log(`${data.uniqueId}: ${data.comment}`);
});