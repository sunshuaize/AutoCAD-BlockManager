using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlockManager.UI.ViewModels
{
    /// <summary>
    /// ViewModel基类，实现INotifyPropertyChanged
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 设置属性值并触发PropertyChanged事件
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">属性字段</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称（自动获取）</param>
        /// <returns>如果值发生变化返回true</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 触发PropertyChanged事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
