// 模拟树形菜单数据
const treeData = [
    {
        id: 'root',
        label: 'Project_CAD',
        type: 'folder',
        isOpen: true,
        children: [
            {
                id: 'architectural',
                label: '建筑施工图',
                type: 'folder',
                isOpen: true,
                children: [
                    { id: 'plans', label: '平面图', type: 'folder', isOpen: false, children: [
                        { id: 'floor1.dwg', label: '一层平面图.dwg', type: 'file' },
                        { id: 'floor2.dwg', label: '二层平面图.dwg', type: 'file' },
                        { id: 'roof.dwg', label: '屋顶平面图.dwg', type: 'file' }
                    ]},
                    { id: 'elevation.dwg', label: '立面图.dwg', type: 'file', active: true },
                    { id: 'section.dwg', label: '剖面图.dwg', type: 'file' }
                ]
            },
            {
                id: 'details',
                label: '节点详图',
                type: 'folder',
                isOpen: false,
                children: [
                    { id: 'stairs.dwg', label: '楼梯详图.dwg', type: 'file' },
                    { id: 'wall_section.dwg', label: '墙身大样.dwg', type: 'file' }
                ]
            },
            {
                id: 'structural',
                label: '结构图',
                type: 'folder',
                isOpen: false,
                children: [
                    { id: 'foundation.dwg', label: '基础平面图.dwg', type: 'file' },
                    { id: 'columns.dwg', label: '柱配筋图.dwg', type: 'file' }
                ]
            },
            { id: 'master_plan.dwg', label: '总平面图.dwg', type: 'file' },
            { id: 'design_spec.dwg', label: '设计说明.dwg', type: 'file' }
        ]
    }
];

// 图标配置 - Windsurf 风格 (线性、极简、无色)
const icons = {
    // 文件夹闭合：简单的箭头或线条，这里用通用的线性文件夹
    folder: `<svg class="w-4 h-4 mr-1.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z"></path></svg>`,
    // 文件夹打开
    folderOpen: `<svg class="w-4 h-4 mr-1.5 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z"></path></svg>`,
    // 文件：通用文档图标
    file: `<svg class="w-4 h-4 mr-1.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path></svg>`,
    // 图片：山峰图标
    image: `<svg class="w-4 h-4 mr-1.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path></svg>`,
    // 箭头 (极简 V 形)
    arrowRight: `<svg class="w-3 h-3 text-gray-400 transform transition-transform duration-200" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>`,
    arrowDown: `<svg class="w-3 h-3 text-gray-400 transform rotate-90 transition-transform duration-200" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>`
};

// 渲染树节点
function renderTree(data, padding = 0) {
    let html = '';
    data.forEach(item => {
        // 处理特殊的 section 类型 (如大纲、时间线)
        if (item.type === 'section') {
             const arrow = item.isOpen ? icons.arrowDown : icons.arrowRight;
             html += `
                <div class="select-none border-t border-gray-100 first:border-t-0">
                    <div class="flex items-center py-1.5 px-4 cursor-pointer hover:bg-gray-100 text-xs font-bold text-gray-600 uppercase tracking-wider transition-colors"
                         onclick="toggleNode('${item.id}')">
                        <span class="mr-1 flex-shrink-0 w-3 h-3 flex items-center justify-center">
                            ${arrow}
                        </span>
                        <span class="truncate">${item.label}</span>
                    </div>
                    ${item.isOpen && item.children ? 
                        `<div class="">
                            ${renderTree(item.children, padding + 8)}
                        </div>` : ''}
                </div>
            `;
            return;
        }

        const isFolder = item.type === 'folder';
        const icon = item.type === 'image' ? icons.image : (isFolder ? (item.isOpen ? icons.folderOpen : icons.folder) : icons.file);
        // 如果是文件，不需要箭头，但是需要占位符以保持对齐
        const arrow = isFolder ? (item.isOpen ? icons.arrowDown : icons.arrowRight) : '<span class="w-3 h-3"></span>';
        // 选中样式：背景变蓝，文字变白 (类似 VS Code 默认选中，或 Windsurf 的浅蓝)
        // Windsurf 选中风格：背景浅蓝，左侧有指示条(这里简化为背景浅蓝)
        const activeClass = item.active ? 'bg-blue-50 text-blue-600' : 'hover:bg-gray-100 text-gray-600';
        
        html += `
            <div class="tree-node select-none">
                <div class="flex items-center py-1 px-2 cursor-pointer ${activeClass} text-[13px] transition-colors border border-transparent ${item.active ? 'border-blue-50' : ''}" 
                     onclick="toggleNode('${item.id}')"
                     style="padding-left: ${padding + 8}px">
                    <span class="mr-1 flex-shrink-0 w-3 h-3 flex items-center justify-center">
                        ${arrow}
                    </span>
                    ${icon}
                    <span class="truncate">${item.label}</span>
                </div>
                ${isFolder && item.isOpen && item.children ? 
                    `<div class="tree-children">
                        ${renderTree(item.children, padding + 12)}
                    </div>` : ''}
            </div>
        `;
    });
    return html;
}

// 切换节点状态
function toggleNode(id) {
    const findAndToggle = (nodes) => {
        for (let node of nodes) {
            if (node.id === id) {
                if (node.type === 'folder' || node.type === 'section') {
                    node.isOpen = !node.isOpen;
                } else {
                    // 如果是文件，设置为选中状态
                    resetActive(treeData);
                    node.active = true;
                    // updatePreview(node.label); // 暂时移除预览更新逻辑，因为右侧现在是静态的 Windsurf 首页
                }
                return true;
            }
            if (node.children) {
                if (findAndToggle(node.children)) return true;
            }
        }
        return false;
    };
    
    findAndToggle(treeData);
    render();
}

// 重置选中状态
function resetActive(nodes) {
    nodes.forEach(node => {
        node.active = false;
        if (node.children) resetActive(node.children);
    });
}

// 主渲染函数
function render() {
    const container = document.getElementById('tree-container');
    if (container) {
        container.innerHTML = renderTree(treeData);
    }
}

// 初始化
document.addEventListener('DOMContentLoaded', () => {
    render();
    initSplitter();
    console.log('Windsurf UI initialized');
});

// 分隔条拖动逻辑
function initSplitter() {
    const sidebar = document.getElementById('sidebar');
    const splitter = document.getElementById('splitter');
    let isResizing = false;
    let startX = 0;
    let startWidth = 0;

    if (!sidebar || !splitter) return;

    splitter.addEventListener('mousedown', (e) => {
        isResizing = true;
        startX = e.clientX;
        startWidth = sidebar.offsetWidth;
        document.body.style.cursor = 'col-resize'; // 全局鼠标样式
        splitter.classList.add('bg-blue-400'); // 高亮分隔条
    });

    document.addEventListener('mousemove', (e) => {
        if (!isResizing) return;
        
        const dx = e.clientX - startX;
        const newWidth = startWidth + dx;
        
        // 限制最小和最大宽度 (与 CSS 中的 min/max-width 保持一致或更严格)
        if (newWidth >= 150 && newWidth <= 500) {
            sidebar.style.width = `${newWidth}px`;
        }
    });

    document.addEventListener('mouseup', () => {
        if (isResizing) {
            isResizing = false;
            document.body.style.cursor = ''; // 恢复鼠标样式
            splitter.classList.remove('bg-blue-400'); // 移除高亮
        }
    });
}
