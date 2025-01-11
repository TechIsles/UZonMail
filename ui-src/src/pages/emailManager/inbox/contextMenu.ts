/* eslint-disable @typescript-eslint/no-explicit-any */
import { IInbox, deleteInboxById, updateInbox } from 'src/api/emailBox'

import { IContextMenuItem } from 'src/components/contextMenu/types'
import { IPopupDialogParams } from 'src/components/popupDialog/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
import { getInboxFields } from './headerFunctions'
import { showDialog } from 'src/components/popupDialog/PopupDialog'

export function useContextMenu (deleteRowById: (id?: number) => void) {
  // 删除发件箱
  async function deleteInbox (row: Record<string, any>) {
    const inbox = row as IInbox
    // 提示是否删除
    const confirm = await confirmOperation('删除收件箱', `是否删除收件箱: ${inbox.email}？`)
    if (!confirm) return

    await deleteInboxById(inbox.id as number)

    // 开始删除
    deleteRowById(inbox.id)

    notifySuccess('删除成功')
  }

  // 更新发件箱
  async function onUpdateInbox (row: Record<string, any>) {
    const inbox = row as IInbox

    const fields = getInboxFields()
    // 修改默认值
    fields.forEach(field => {
      switch (field.name) {
        case 'email':
          field.value = inbox.email
          break
        case 'name':
          field.value = inbox.name
          break
        case 'minInboxCooldownHours':
          field.value = inbox.minInboxCooldownHours
          break
        case 'description':
          field.value = inbox.description
          break
        default:
          break
      }
    })

    // 新增发件箱
    const popupParams: IPopupDialogParams = {
      title: `修改收件箱 / ${inbox.email}`,
      fields
    }

    // 弹出对话框
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const { ok, data } = await showDialog<IInbox>(popupParams)
    if (!ok) return

    // 向服务器传递更新
    await updateInbox(inbox.id as number, data)

    // 将参数更新到 inbox 中
    Object.assign(inbox, data, { decryptedPassword: false })

    notifySuccess('更新成功')
  }

  const inboxContextMenuItems: IContextMenuItem[] = [
    {
      name: 'edit',
      label: '编辑',
      tooltip: '编辑当前发件箱',
      onClick: onUpdateInbox
    },
    {
      name: 'delete',
      label: '删除',
      tooltip: '删除当前发件箱',
      color: 'negative',
      onClick: deleteInbox
    }
  ]

  return { inboxContextMenuItems }
}
