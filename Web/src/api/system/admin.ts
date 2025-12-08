import request from '/@/utils/request';
enum Api {
	DictTypeDataList = '/api/sysDictData/DataList',
	AllDictList = '/api/sysDictType/AllDictList',
	HardwareInfo = '/api/sysServer/hardwareInfo',
	RuntimeInfo = '/api/sysServer/runtimeInfo',
	NuGetPackagesInfo = '/api/sysServer/nuGetPackagesInfo',
}

// 根据字典类型编码获取字典值集合
export const getDictDataList = (params?: any) =>
	request({
		url: `${Api.DictTypeDataList}/${params}`,
		method: 'get',
	});

// 获取所有字典
export const getAllDictList = () =>
	request({
		url: `${Api.AllDictList}`,
		method: 'get',
	});

	// 获取硬件信息
export const getHardwareInfo = () =>
	request({
		url: `${Api.HardwareInfo}`,
		method: 'post',
	});

// 获取运行时信息
export const getRuntimeInfo = () =>
	request({
		url: `${Api.RuntimeInfo}`,
		method: 'post',
	});

// 获取NuGet包信息
export const getNuGetPackagesInfo = () =>
	request({
		url: `${Api.NuGetPackagesInfo}`,
		method: 'post',
	});

