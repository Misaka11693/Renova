import * as SignalR from '@microsoft/signalr';
import { ElNotification } from 'element-plus';
import { getToken } from '/@/utils/axios-utils';

// 初始化SignalR对象
const connection = new SignalR.HubConnectionBuilder()
	.configureLogging(SignalR.LogLevel.Information)
	.withUrl(`${window.__env__.VITE_API_URL}/hubs/onlineUser?token=${getToken()}`, { transport: SignalR.HttpTransportType.WebSockets, skipNegotiation: true })
	.withAutomaticReconnect({
		nextRetryDelayInMilliseconds: () => {
			return 5000; // 每5秒重连一次
		},
	})
	.build();

// 心跳检测：若15s内没有向服务器发送任何消息，则ping一下服务器端
connection.keepAliveIntervalInMilliseconds = 15 * 1000;
// 超时时间：若30s内没有收到服务器端发过来的信息，则认为服务器端异常
connection.serverTimeoutInMilliseconds = 30 * 1000;

// 启动连接
connection.start().then(() => {
	console.log('启动连接');
});
// 断开连接
connection.onclose(async () => {
	console.log('断开连接');
});
// 重连中
connection.onreconnecting(() => {
	ElNotification({
		title: '提示',
		message: '服务器已断线...',
		type: 'error',
		position: 'bottom-right',
	});
});
// 重连成功
connection.onreconnected(() => {
	console.log('重连成功');
});

connection.on('OnlineUserList', () => {});

export { connection as signalR };
