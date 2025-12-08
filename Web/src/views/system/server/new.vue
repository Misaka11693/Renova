<template>
	<div class="device-info-container">
		<!-- 系统概览卡片 -->
		<el-row :gutter="16" style="margin-bottom: 16px">
			<el-col :span="24">
				<el-card class="overview-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-monitor"></i>
							<span>系统概览</span>
							<span class="status-text">运行正常</span>
						</div>
					</template>
					<div class="overview-content">
						<div class="overview-item">
							<div class="overview-icon cpu-icon">
								<i class="el-icon-cpu"></i>
							</div>
							<div class="overview-text">
								<div class="overview-title">处理器</div>
								<div class="overview-value">{{ deviceInfo.cpuInfo.processorName }}</div>
								<div class="overview-subtitle">{{ deviceInfo.cpuInfo.physicalCoreCount }}核 {{ deviceInfo.cpuInfo.logicalCoreCount }}线程</div>
							</div>
						</div>
						<div class="overview-item">
							<div class="overview-icon ram-icon">
								<i class="el-icon-film"></i>
							</div>
							<div class="overview-text">
								<div class="overview-title">内存</div>
								<div class="overview-value">{{ deviceInfo.ramInfo.totalSpace }}</div>
								<div class="overview-subtitle">使用率 {{ deviceInfo.ramInfo.usagePercentage }}%</div>
							</div>
						</div>
						<div class="overview-item">
							<div class="overview-icon os-icon">
								<i class="el-icon-monitor"></i>
							</div>
							<div class="overview-text">
								<div class="overview-title">操作系统</div>
								<div class="overview-value">{{ deviceInfo.systemInfo.osDescription }}</div>
								<div class="overview-subtitle">{{ deviceInfo.systemInfo.osArchitecture }} 架构</div>
							</div>
						</div>
						<div class="overview-item">
							<div class="overview-icon framework-icon">
								<i class="el-icon-guide"></i>
							</div>
							<div class="overview-text">
								<div class="overview-title">运行框架</div>
								<div class="overview-value">{{ deviceInfo.systemInfo.frameworkDescription }}</div>
								<div class="overview-subtitle">{{ deviceInfo.systemInfo.machineName }}</div>
							</div>
						</div>
						<div class="overview-item">
							<div class="overview-icon time-icon">
								<i class="el-icon-time"></i>
							</div>
							<div class="overview-text">
								<div class="overview-title">运行时间</div>
								<div class="overview-value">{{ deviceInfo.systemUptime }}</div>
								<div class="overview-subtitle">系统稳定运行</div>
							</div>
						</div>
					</div>
				</el-card>
			</el-col>
		</el-row>

		<!-- CPU和内存性能 -->
		<el-row :gutter="16" style="margin-bottom: 16px">
			<el-col :md="12" :sm="24">
				<el-card class="performance-card cpu-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-cpu"></i>
							<span>CPU 性能</span>
						</div>
					</template>
					<div class="performance-content">
						<div class="performance-chart">
							<el-progress type="dashboard" :percentage="deviceInfo.cpuInfo.usagePercentage" :color="getCpuColor(deviceInfo.cpuInfo.usagePercentage)" :width="180" :stroke-width="12">
								<template #default>
									<div class="progress-content">
										<div class="progress-value">{{ deviceInfo.cpuInfo.usagePercentage }}%</div>
										<div class="progress-label">CPU使用率</div>
									</div>
								</template>
							</el-progress>
						</div>
						<div class="performance-details">
							<div class="detail-item">
								<span class="detail-label">处理器架构：</span>
								<span class="detail-value">{{ deviceInfo.cpuInfo.processorArchitecture }}</span>
							</div>
							<div class="detail-item">
								<span class="detail-label">基础频率：</span>
								<span class="detail-value">{{ deviceInfo.cpuInfo.baseClockSpeed }} GHz</span>
							</div>
							<div class="detail-item">
								<span class="detail-label">缓存大小：</span>
								<span class="detail-value">{{ deviceInfo.cpuInfo.cacheSize }}</span>
							</div>
						</div>
					</div>
				</el-card>
			</el-col>
			<el-col :md="12" :sm="24">
				<el-card class="performance-card ram-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-memory-card"></i>
							<span>内存使用</span>
						</div>
					</template>
					<div class="performance-content">
						<div class="performance-chart">
							<el-progress type="dashboard" :percentage="deviceInfo.ramInfo.usagePercentage" :color="getRamColor(deviceInfo.ramInfo.usagePercentage)" :width="180" :stroke-width="12">
								<template #default>
									<div class="progress-content">
										<div class="progress-value">{{ deviceInfo.ramInfo.usagePercentage.toFixed(1) }}%</div>
										<div class="progress-label">内存使用</div>
									</div>
								</template>
							</el-progress>
						</div>
						<div class="performance-details">
							<div class="detail-item">
								<span class="detail-label">总内存：</span>
								<span class="detail-value">{{ deviceInfo.ramInfo.totalSpace }}</span>
							</div>
							<div class="detail-item">
								<span class="detail-label">已使用：</span>
								<span class="detail-value">{{ deviceInfo.ramInfo.usedSpace }}</span>
							</div>
							<div class="detail-item">
								<span class="detail-label">可用：</span>
								<span class="detail-value">{{ deviceInfo.ramInfo.freeSpace }}</span>
							</div>
						</div>
					</div>
				</el-card>
			</el-col>
		</el-row>

		<!-- 磁盘信息 -->
		<el-row :gutter="16" style="margin-bottom: 16px">
			<el-col :span="24">
				<el-card class="disk-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-files"></i>
							<span>磁盘使用情况</span>
						</div>
					</template>
					<el-row :gutter="16">
						<el-col :md="12" :sm="24" v-for="disk in deviceInfo.diskInfos" :key="disk.diskName">
							<div class="disk-item">
								<div class="disk-header">
									<div class="disk-name">{{ disk.diskName }}</div>
									<div class="disk-type">{{ disk.typeName }}</div>
								</div>
								<div class="disk-content">
									<div class="disk-chart">
										<el-progress type="circle" :percentage="parseFloat(disk.usedPercentage)" :color="getDiskColor(disk.availableRate)" :width="120" :stroke-width="8">
											<template #default>
												<div class="disk-progress-content">
													<div class="disk-usage">{{ disk.usedPercentage }}%</div>
													<div class="disk-label">已使用</div>
												</div>
											</template>
										</el-progress>
									</div>
									<div class="disk-details">
										<div class="disk-detail-item">
											<span class="detail-label">总容量：</span>
											<span class="detail-value">{{ disk.totalSpace }}</span>
										</div>
										<div class="disk-detail-item">
											<span class="detail-label">已使用：</span>
											<span class="detail-value">{{ disk.usedSpace }}</span>
										</div>
										<div class="disk-detail-item">
											<span class="detail-label">可用空间：</span>
											<span class="detail-value">{{ disk.freeSpace }}</span>
										</div>
									</div>
								</div>
							</div>
						</el-col>
					</el-row>
				</el-card>
			</el-col>
		</el-row>

		<!-- GPU信息 -->
		<el-row :gutter="16" style="margin-bottom: 16px">
			<el-col :span="24">
				<el-card class="gpu-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-view"></i>
							<span>显卡信息</span>
						</div>
					</template>
					<el-row :gutter="16">
						<el-col :md="12" :sm="24" v-for="(gpu, index) in deviceInfo.gpuInfos" :key="index">
							<div class="gpu-item">
								<div class="gpu-header">
									<div class="gpu-name">{{ gpu.name }}</div>
									<el-tag :type="gpu.status === 'OK' ? 'success' : 'danger'" size="small">
										{{ gpu.status }}
									</el-tag>
								</div>
								<div class="gpu-details">
									<div class="gpu-detail-item">
										<span class="detail-label">显存：</span>
										<span class="detail-value">{{ gpu.memorySize }}</span>
									</div>
									<div class="gpu-detail-item">
										<span class="detail-label">驱动版本：</span>
										<span class="detail-value">{{ gpu.driverVersion || 'N/A' }}</span>
									</div>
									<div class="gpu-detail-item">
										<span class="detail-label">分辨率：</span>
										<span class="detail-value">{{ gpu.videoModeDescription }}</span>
									</div>
								</div>
							</div>
						</el-col>
					</el-row>
				</el-card>
			</el-col>
		</el-row>

		<!-- 网络信息 -->
		<el-row :gutter="16" style="margin-bottom: 16px">
			<el-col :span="24">
				<el-card class="network-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-connection"></i>
							<span>网络适配器</span>
							<span class="status-text">{{ getActiveNetworks().length }} 个活跃</span>
						</div>
					</template>
					<div class="network-content">
						<el-collapse v-model="activeNetworkNames" accordion>
							<el-collapse-item v-for="network in getActiveNetworks()" :key="network.name" :name="network.name">
								<template #title>
									<div class="network-title">
										<div class="network-status">
											<i :class="getNetworkStatusIcon(network.operationalStatus)"></i>
											<span class="network-name">{{ network.name }}</span>
										</div>
										<div class="network-type">
											<el-tag size="small" :type="getNetworkTypeColor(network.type)">
												{{ network.type }}
											</el-tag>
										</div>
									</div>
								</template>
								<div class="network-details">
									<el-row :gutter="16">
										<el-col :md="12" :sm="24">
											<div class="network-info">
												<div class="network-detail-item">
													<span class="detail-label">描述：</span>
													<span class="detail-value">{{ network.description }}</span>
												</div>
												<div class="network-detail-item">
													<span class="detail-label">物理地址：</span>
													<span class="detail-value">{{ network.physicalAddress || 'N/A' }}</span>
												</div>
												<div class="network-detail-item">
													<span class="detail-label">速度：</span>
													<span class="detail-value">{{ network.speed }}</span>
												</div>
												<div class="network-detail-item" v-if="network.iPv4Addresses && network.iPv4Addresses.length > 0">
													<span class="detail-label">IPv4地址：</span>
													<div class="ip-addresses">
														<el-tag v-for="ip in network.iPv4Addresses" :key="ip.address" size="small" class="ip-tag">
															{{ ip.address }}
														</el-tag>
													</div>
												</div>
											</div>
										</el-col>
										<el-col :md="12" :sm="24" v-if="network.statistics">
											<div class="network-statistics">
												<h4>网络统计</h4>
												<div class="stats-grid">
													<div class="stat-item">
														<div class="stat-label">接收字节</div>
														<div class="stat-value">{{ formatBytes(network.statistics.bytesReceived) }}</div>
													</div>
													<div class="stat-item">
														<div class="stat-label">发送字节</div>
														<div class="stat-value">{{ formatBytes(network.statistics.bytesSent) }}</div>
													</div>
													<div class="stat-item">
														<div class="stat-label">接收包数</div>
														<div class="stat-value">{{ network.statistics.packetsReceived.toLocaleString() }}</div>
													</div>
													<div class="stat-item">
														<div class="stat-label">发送包数</div>
														<div class="stat-value">{{ network.statistics.packetsSent.toLocaleString() }}</div>
													</div>
												</div>
											</div>
										</el-col>
									</el-row>
								</div>
							</el-collapse-item>
						</el-collapse>
					</div>
				</el-card>
			</el-col>
		</el-row>

		<!-- 主板信息 -->
		<el-row :gutter="16" style="margin-bottom: 16px">
			<el-col :span="24">
				<el-card class="board-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-cpu"></i>
							<span>主板信息</span>
						</div>
					</template>
					<div class="board-content">
						<div class="board-item">
							<span class="detail-label">制造商：</span>
							<span class="detail-value">{{ deviceInfo.boardInfo.manufacturer }}</span>
						</div>
						<div class="board-item">
							<span class="detail-label">产品型号：</span>
							<span class="detail-value">{{ deviceInfo.boardInfo.product }}</span>
						</div>
						<div class="board-item">
							<span class="detail-label">版本：</span>
							<span class="detail-value">{{ deviceInfo.boardInfo.version }}</span>
						</div>
						<div class="board-item">
							<span class="detail-label">序列号：</span>
							<span class="detail-value">{{ deviceInfo.boardInfo.serialNumber }}</span>
						</div>
					</div>
				</el-card>
			</el-col>
		</el-row>

		<!-- 系统信息 -->
		<el-row :gutter="16">
			<el-col :span="24">
				<el-card class="system-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-setting"></i>
							<span>系统信息</span>
						</div>
					</template>
					<div class="system-content">
						<div class="system-item">
							<span class="detail-label">操作系统：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.osDescription }}</span>
						</div>
						<div class="system-item">
							<span class="detail-label">系统版本：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.osVersion }}</span>
						</div>
						<div class="system-item">
							<span class="detail-label">系统架构：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.osArchitecture }}</span>
						</div>
						<div class="system-item">
							<span class="detail-label">运行框架：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.frameworkDescription }}</span>
						</div>
						<div class="system-item">
							<span class="detail-label">机器名称：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.machineName }}</span>
						</div>
						<div class="system-item">
							<span class="detail-label">当前用户：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.userName }}</span>
						</div>
						<div class="system-item">
							<span class="detail-label">系统启动时间：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.systemStartTime }}</span>
						</div>
						<div class="system-item">
							<span class="detail-label">进程启动时间：</span>
							<span class="detail-value">{{ deviceInfo.systemInfo.processStartTime }}</span>
						</div>
					</div>
				</el-card>
			</el-col>
		</el-row>

		<!-- NuGet包信息 -->
		<el-row :gutter="16">
			<el-col :span="24">
				<el-card class="nuget-card" shadow="hover">
					<template #header>
						<div class="card-header">
							<i class="el-icon-collection"></i>
							<span>NuGet 包信息</span>
							<span class="status-text">{{ deviceInfo.nugetPackages.length }} 个包</span>
						</div>
					</template>
					<div class="nuget-content">
						<div v-for="pkg in deviceInfo.nugetPackages" :key="pkg.packageName" class="package-item">
							<el-tag round>
								<div class="package-info">
									<div class="package-name">{{ pkg.packageName }}</div>
									<div class="package-version">v{{ pkg.packageVersion }}</div>
								</div>
							</el-tag>
						</div>
					</div>
				</el-card>
			</el-col>
		</el-row>
	</div>
</template>

<script lang="ts" setup name="deviceInfo">
import { ref, reactive, onMounted } from 'vue';
import { getHardwareInfo, getRuntimeInfo, getNuGetPackagesInfo } from '/@/api/system/admin';

// 网络接口类型定义
interface NetworkInfo {
	name: string;
	description: string;
	type: string;
	operationalStatus: string;
	speed: string;
	physicalAddress: string;
	supportsMulticast: boolean;
	isReceiveOnly: boolean;
	dnsAddresses: string[];
	gatewayAddresses: string[];
	dhcpServerAddresses: string[];
	iPv4Addresses: Array<{
		address: string;
		subnetMask: string;
		prefixLength: number;
	}>;
	iPv6Addresses: Array<{
		address: string;
		subnetMask: string;
		prefixLength: number;
	}>;
	statistics?: {
		bytesReceived: number;
		bytesSent: number;
		packetsReceived: number;
		packetsSent: number;
		incomingPacketsDiscarded: number;
		outgoingPacketsDiscarded: number;
		incomingPacketsWithErrors: number;
		outgoingPacketsWithErrors: number;
	};
}

// 设备信息数据
const deviceInfo = reactive<any>({
	cpuInfo: {
		processorName: '',
		processorArchitecture: '',
		physicalCoreCount: 0,
		logicalCoreCount: 0,
		baseClockSpeed: 0,
		cacheSize: '',
		usagePercentage: 0,
	},
	ramInfo: {
		totalSpace: '',
		usedSpace: '',
		freeSpace: '',
		usagePercentage: 0,
	},
	diskInfos: [],
	networkInfos: [],
	gpuInfos: [],
	boardInfo: {
		manufacturer: '',
		product: '',
		version: '',
		serialNumber: '',
	},
	systemUptime: '',
	systemInfo: {
		osDescription: '',
		osVersion: '',
		osArchitecture: '',
		frameworkDescription: '',
		machineName: '',
		userName: '',
		systemStartTime: '',
		processStartTime: '',
	},
	nugetPackages: [],
});

const activeNetworkNames = ref('');

// 格式化字节数为可读格式
const formatBytes = (bytes: number) => {
	if (bytes === 0) return '0 B';
	const k = 1024;
	const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
	const i = Math.floor(Math.log(bytes) / Math.log(k));
	return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

// 获取硬件信息
const getHardwareInfoData = async () => {
	try {
		// 并行获取硬件信息、运行时信息和 NuGet 包信息
		const [hardwareRes, runtimeRes, nugetRes] = await Promise.all([getHardwareInfo(), getRuntimeInfo(), getNuGetPackagesInfo()]);

		const hardwareData = hardwareRes.data.result;
		const runtimeData = runtimeRes.data.result;
		const nugetPackages = nugetRes.data.result;

		// 处理CPU信息
		Object.assign(deviceInfo.cpuInfo, {
			processorName: hardwareData.cpuInfo.processorName,
			processorArchitecture: hardwareData.cpuInfo.processorArchitecture,
			physicalCoreCount: hardwareData.cpuInfo.physicalCoreCount,
			logicalCoreCount: hardwareData.cpuInfo.logicalCoreCount,
			baseClockSpeed: hardwareData.cpuInfo.baseClockSpeed,
			cacheSize: formatBytes(hardwareData.cpuInfo.cacheBytes || 0),
			usagePercentage: hardwareData.cpuInfo.usagePercentage,
		});

		// 处理内存信息
		Object.assign(deviceInfo.ramInfo, {
			totalSpace: formatBytes(hardwareData.ramInfo.totalBytes),
			usedSpace: formatBytes(hardwareData.ramInfo.usedBytes),
			freeSpace: formatBytes(hardwareData.ramInfo.freeBytes),
			usagePercentage: hardwareData.ramInfo.usagePercentage,
		});

		// 处理磁盘信息
		deviceInfo.diskInfos = hardwareData.diskInfos.map((disk: any) => ({
			diskName: disk.diskName,
			typeName: disk.typeName,
			totalSpace: formatBytes(disk.totalSpace),
			usedSpace: formatBytes(disk.usedSpace),
			freeSpace: formatBytes(disk.freeSpace),
			availableRate: disk.availableRate,
			usedPercentage: ((disk.usedSpace / disk.totalSpace) * 100).toFixed(1),
		}));

		// 处理网络信息
		deviceInfo.networkInfos = hardwareData.networkInfos;

		// 处理GPU信息
		deviceInfo.gpuInfos = hardwareData.gpuInfos.map((gpu: any) => ({
			name: gpu.name,
			description: gpu.description,
			vendor: gpu.vendor,
			deviceId: gpu.deviceId,
			busInfo: gpu.busInfo,
			driverVersion: gpu.driverVersion,
			memorySize: gpu.memoryBytes ? formatBytes(gpu.memoryBytes) : 'N/A',
			videoModeDescription: gpu.videoModeDescription,
			status: gpu.status,
		}));

		// 处理主板信息
		Object.assign(deviceInfo.boardInfo, {
			manufacturer: hardwareData.boardInfo.manufacturer,
			product: hardwareData.boardInfo.product,
			version: hardwareData.boardInfo.version,
			serialNumber: hardwareData.boardInfo.serialNumber,
		});

		// 处理系统运行时间和其他系统信息
		deviceInfo.systemUptime = runtimeData.runningTime || runtimeData.runtimeInfo.systemUptime;

		// 添加系统信息到deviceInfo（可选）
		deviceInfo.systemInfo = {
			osDescription: runtimeData.runtimeInfo.osDescription,
			osVersion: runtimeData.runtimeInfo.osVersion,
			osArchitecture: runtimeData.runtimeInfo.osArchitecture,
			frameworkDescription: runtimeData.runtimeInfo.frameworkDescription,
			machineName: runtimeData.runtimeInfo.machineName,
			userName: runtimeData.runtimeInfo.userName,
			systemStartTime: runtimeData.runtimeInfo.systemStartTime,
			processStartTime: runtimeData.runtimeInfo.processStartTime,
		};

		// 处理 NuGet 包信息
		deviceInfo.nugetPackages = nugetPackages.map((pkg: any) => ({
			packageName: pkg.packageName,
			packageVersion: pkg.packageVersion,
		}));
	} catch (error) {
		console.error('获取硬件信息失败:', error);
		// 设置默认值，避免页面崩溃
		deviceInfo.systemUptime = '获取失败';
	}
};

// 获取活跃网络
const getActiveNetworks = () => {
	return deviceInfo.networkInfos.filter(
		(network: NetworkInfo) =>
			network.operationalStatus === 'Up' && !network.name.includes('WFP') && !network.name.includes('QoS') && !network.name.includes('Filter') && !network.name.includes('vSwitch')
	);
};

// 获取网络状态图标
const getNetworkStatusIcon = (status: string) => {
	switch (status) {
		case 'Up':
			return 'el-icon-success network-status-up';
		case 'Down':
			return 'el-icon-error network-status-down';
		default:
			return 'el-icon-warning network-status-unknown';
	}
};

// 获取网络类型颜色
const getNetworkTypeColor = (type: string) => {
	switch (type) {
		case 'Ethernet':
			return 'primary';
		case 'Wireless80211':
			return 'success';
		case 'Loopback':
			return 'info';
		default:
			return 'warning';
	}
};

// 获取CPU使用率颜色
const getCpuColor = (percentage: number) => {
	if (percentage < 50) return '#67c23a';
	if (percentage < 80) return '#e6a23c';
	return '#f56c6c';
};

// 获取内存使用率颜色
const getRamColor = (percentage: number) => {
	if (percentage < 60) return '#409eff';
	if (percentage < 80) return '#e6a23c';
	return '#f56c6c';
};

// 获取磁盘使用率颜色
const getDiskColor = (availablePercentage: number) => {
	const usedPercentage = 100 - availablePercentage;
	if (usedPercentage < 70) return '#67c23a';
	if (usedPercentage < 90) return '#e6a23c';
	return '#f56c6c';
};

onMounted(() => {
	console.log('设备信息页面已加载');
	getHardwareInfoData();
});
</script>

<style lang="scss" scoped>
.device-info-container {
	padding: 16px;
	background: #f5f7fa;
	min-height: 100vh;
}

.card-header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	font-weight: 600;

	i {
		margin-right: 8px;
		font-size: 18px;
	}

	> span:first-of-type {
		flex: 1;
	}
}

.status-text {
	font-size: 12px;
	color: #67c23a;
	flex-shrink: 0;
}

// 概览卡片
.overview-card {
	background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
	color: white;

	:deep(.el-card__header) {
		background: rgba(255, 255, 255, 0.1);
		border-bottom: 1px solid rgba(255, 255, 255, 0.2);
		color: white;
	}
}

.overview-content {
	display: flex;
	justify-content: space-around;
	flex-wrap: wrap;
	gap: 20px;
}

.overview-item {
	display: flex;
	align-items: center;
	gap: 16px;
	min-width: 200px;
	flex: 1;
	min-height: 80px;
	max-height: 103px;
}

.overview-icon {
	width: 60px;
	height: 60px;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 24px;
	background: rgba(255, 255, 255, 0.2);
	flex-shrink: 0;
}

.overview-text {
	flex: 1;
	min-width: 0;
	display: flex;
	flex-direction: column;
	justify-content: center;
	min-height: 72px;
	max-height: 95px;

	.overview-title {
		font-size: 14px;
		opacity: 0.8;
		margin-bottom: 4px;
		line-height: 1.2;
		height: 17px;
		display: flex;
		align-items: center;
	}

	.overview-value {
		font-size: 18px;
		font-weight: 600;
		margin-bottom: 4px;
		word-break: break-word;
		line-height: 1.3;
		min-height: 23px;
		max-height: 46px;
		display: -webkit-box;
		-webkit-line-clamp: 2;
		-webkit-box-orient: vertical;
		overflow: hidden;
		text-overflow: ellipsis;
	}

	.overview-subtitle {
		font-size: 12px;
		opacity: 0.7;
		line-height: 1.2;
		height: 15px;
		display: flex;
		align-items: center;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}
}

// 性能卡片
.performance-card {
	&.cpu-card {
		border-left: 4px solid #e6a23c;
	}

	&.ram-card {
		border-left: 4px solid #409eff;
	}
}

.performance-content {
	display: flex;
	align-items: center;
	gap: 24px;
}

.performance-chart {
	flex-shrink: 0;
}

.progress-content {
	text-align: center;

	.progress-value {
		font-size: 24px;
		font-weight: 600;
		color: #303133;
	}

	.progress-label {
		font-size: 12px;
		color: #909399;
		margin-top: 4px;
	}
}

.performance-details {
	flex: 1;
}

.detail-item {
	display: flex;
	justify-content: space-between;
	padding: 8px 0;
	border-bottom: 1px solid #f0f0f0;

	&:last-child {
		border-bottom: none;
	}
}

.detail-label {
	color: #606266;
	font-size: 14px;
}

.detail-value {
	color: #303133;
	font-weight: 500;
	font-size: 14px;
}

// 磁盘卡片
.disk-card {
	border-left: 4px solid #67c23a;
}

.disk-item {
	background: #f8f9fa;
	border-radius: 8px;
	padding: 16px;
	margin-bottom: 16px;

	&:last-child {
		margin-bottom: 0;
	}
}

.disk-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;
}

.disk-name {
	font-size: 18px;
	font-weight: 600;
	color: #303133;
}

.disk-type {
	color: #909399;
	font-size: 14px;
}

.disk-content {
	display: flex;
	align-items: center;
	gap: 24px;
}

.disk-chart {
	flex-shrink: 0;
}

.disk-progress-content {
	text-align: center;

	.disk-usage {
		font-size: 16px;
		font-weight: 600;
		color: #303133;
	}

	.disk-label {
		font-size: 12px;
		color: #909399;
	}
}

.disk-details {
	flex: 1;
}

.disk-detail-item {
	display: flex;
	justify-content: space-between;
	padding: 6px 0;
	border-bottom: 1px solid #e4e7ed;

	&:last-child {
		border-bottom: none;
	}
}

// GPU卡片
.gpu-card {
	border-left: 4px solid #f56c6c;
}

.gpu-item {
	background: #f8f9fa;
	border-radius: 8px;
	padding: 16px;
	margin-bottom: 16px;

	&:last-child {
		margin-bottom: 0;
	}
}

.gpu-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 12px;
}

.gpu-name {
	font-size: 16px;
	font-weight: 600;
	color: #303133;
	flex: 1;
	margin-right: 12px;
}

.gpu-details {
	display: grid;
	gap: 8px;
}

.gpu-detail-item {
	display: flex;
	justify-content: space-between;
	padding: 4px 0;
}

// 网络卡片
.network-card {
	border-left: 4px solid #909399;
}

.network-title {
	display: flex;
	justify-content: space-between;
	align-items: center;
	width: 100%;
	padding-right: 16px;
}

.network-status {
	display: flex;
	align-items: center;
	gap: 8px;
}

.network-name {
	font-weight: 500;
}

.network-status-up {
	color: #67c23a;
}

.network-status-down {
	color: #f56c6c;
}

.network-status-unknown {
	color: #e6a23c;
}

.network-details {
	padding: 16px 0;
}

.network-info {
	.network-detail-item {
		margin-bottom: 12px;

		&:last-child {
			margin-bottom: 0;
		}
	}
}

.ip-addresses {
	margin-top: 4px;
}

.ip-tag {
	margin-right: 8px;
	margin-bottom: 4px;
}

.network-statistics {
	h4 {
		margin: 0 0 16px 0;
		color: #303133;
	}
}

.stats-grid {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 12px;
}

.stat-item {
	background: #f0f2f5;
	padding: 12px;
	border-radius: 6px;
	text-align: center;
}

.stat-label {
	font-size: 12px;
	color: #909399;
	margin-bottom: 4px;
}

.stat-value {
	font-size: 14px;
	font-weight: 600;
	color: #303133;
}

// 主板卡片
.board-card {
	border-left: 4px solid #973399;
}

.board-content {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
	gap: 16px;
}

.board-item {
	display: flex;
	justify-content: space-between;
	padding: 12px;
	background: #f8f9fa;
	border-radius: 6px;
}

// 系统信息卡片
.system-card {
	border-left: 4px solid #409eff;
}

.system-content {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
	gap: 16px;
}

.system-item {
	display: flex;
	justify-content: space-between;
	padding: 12px;
	background: #f8f9fa;
	border-radius: 6px;
}

// 响应式设计
@media (max-width: 768px) {
	.overview-content {
		flex-direction: column;
	}

	.overview-item {
		min-width: 100%;
	}

	.performance-content {
		flex-direction: column;
		text-align: center;
	}

	.disk-content {
		flex-direction: column;
		text-align: center;
	}

	.stats-grid {
		grid-template-columns: 1fr;
	}

	.board-content {
		grid-template-columns: 1fr;
	}
}

// 动画效果
.el-card {
	transition:
		transform 0.3s ease,
		box-shadow 0.3s ease;

	&:hover {
		transform: translateY(-2px);
		box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
	}
}

.el-progress {
	:deep(.el-progress__text) {
		color: inherit !important;
	}
}

:deep(.el-collapse-item__header) {
	padding-left: 0;
	padding-right: 16px;
}

:deep(.el-collapse-item__content) {
	padding-bottom: 16px;
}

// NuGet包信息卡片样式
.nuget-card {
	border-left: 4px solid #409eff;
	margin-top: 16px;
}

.nuget-content {
	padding: 8px;
	display: flex;
	flex-wrap: wrap;
	gap: 8px;
}

.package-item {
	display: inline-block;
	margin: 4px;
	text-align: left;
}

.package-info {
	display: inline-flex;
	align-items: center;
	gap: 4px;
}

.package-name {
	font-family: 'Consolas', monospace;
}

.package-version {
	color: #909399;
	font-size: 9px;
}

:deep(.el-tag) {
	--el-tag-bg-color: var(--el-color-primary-light-9);
	--el-tag-border-color: var(--el-color-primary-light-8);
	--el-tag-hover-color: var(--el-color-primary);

	&:hover {
		background-color: var(--el-color-primary-light-8);
	}
}
</style>
