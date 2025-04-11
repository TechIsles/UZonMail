import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { notifyError, notifySuccess, showDialog } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'
import { useUserInfoStore } from 'src/stores/user'
import type { IEmailGroupListItem } from '../components/types'

import logger from 'loglevel'
import { splitString } from 'src/utils/stringHelper'
import type { ISmtpInfo } from 'src/api/smtpInfo';
import { GuessSmtpInfoPost } from 'src/api/smtpInfo'
import type { IOutbox } from 'src/api/emailBox';
import { createOutboxes } from 'src/api/emailBox'
import { aes } from 'src/utils/encrypt'

/**
 * 从 txt 文件导入邮件
 * 这种方法，需要智能计算邮件的 smtp 及端口号
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function useOutboxImporter (emailGroup: Ref<IEmailGroupListItem>, addNewRow: (newRow: Record<string, any>) => void) {
  const { t } = useI18n()
  const userInfoStore = useUserInfoStore()

  // #region 从文本导入
  async function onImportOutboxFromTxt (emailGroupId: number | null = null) {
    if (typeof emailGroupId !== 'number') emailGroupId = emailGroup.value.id as number

    // 新建弹窗
    const popupParams: IPopupDialogParams = {
      title: `从文本导入发件箱 / ${emailGroupId}`,
      oneColumn: true,
      fields: [
        {
          name: 'text',
          label: '发件箱文本',
          type: PopupDialogFieldType.textarea,
          placeholder: '每行一个发件箱',
          value: '',
          required: true,
          disableAutogrow: true
        }
      ]
    }

    const result = await showDialog(popupParams)
    if (!result.ok) return

    // 开始解析导入的内容
    const outboxTexts: string[][] = result.data.text.split('\n')
      .map((x: string) => x.trim())
      .filter((x: string) => x.length > 0)
      .map((x: string) => {
        return splitString(x)
      })
      .filter((x: string[]) => x.length > 1)

    if (outboxTexts.length === 0) {
      notifyError('未找到可导入的数据')
      return
    }

    // 解析其中的邮箱部分，向服务器请求补全默认值
    logger.debug('[useOutboxImporter] outboxTexts', outboxTexts)

    const emails = outboxTexts.map(x => x.find(y => y.includes('@')))
    const { data: smtpInfos } = await GuessSmtpInfoPost(emails as string[])

    const newData: IOutbox[] = []
    for (const outboxText of outboxTexts) {
      const outbox = __getNewOutboxData(emailGroupId, outboxText, smtpInfos)
      if (outbox) newData.push(outbox)
    }

    // 向服务器请求新增
    const { data: outboxes } = await createOutboxes(newData)

    if (emailGroupId === emailGroup.value.id) {
      outboxes.forEach(x => {
        addNewRow(x)
      })
    }

    notifySuccess('导入成功')
  }

  function __getNewOutboxData (emailGroupId: number, outboxTexts: string[], smtpInfos: ISmtpInfo[]): IOutbox | null {
    const outbox: IOutbox = {
      emailGroupId,
      email: '',
      smtpHost: '',
      smtpPort: 0,
      userName: '',
      password: '',
      enableSSL: true
    }

    // 获取邮箱
    const email = outboxTexts.find(x => x.includes('@'))
    if (!email) return null
    outbox.email = email

    const smtpPassword = outboxTexts.find(x => !x.includes('@') && !x.includes("smtp.") && isNaN(Number(x)))
    if (!smtpPassword) return null
    outbox.password = smtpPassword

    const smtpHost = outboxTexts.find(x => x.includes('smtp.'))
    if (smtpHost) outbox.smtpHost = smtpHost

    const smtpPort = outboxTexts.find(x => Number(x) < 65536 && Number(x) > 0)
    if (smtpPort) outbox.smtpPort = Number(smtpPort)

    // 对明文密码加密
    outbox.password = aes(userInfoStore.smtpPasswordSecretKeys[0] as string, userInfoStore.smtpPasswordSecretKeys[1] as string, outbox.password)

    // 添加其它项
    const smtpInfo = smtpInfos.find(x => x.domain === outbox.email.split('@')[1])
    if (!smtpInfo) return outbox

    // 开始补充数据
    if (!outbox.smtpHost) outbox.smtpHost = smtpInfo.host
    if (!outbox.smtpPort) outbox.smtpPort = smtpInfo.port
    outbox.enableSSL = smtpInfo.enableSSL

    return outbox
  }

  const importFromTxtLable = t('outboxManager.importFromTxt')
  const importFromTxtTooltip = t('outboxManager.importFromTxtTooltip').split('\n')
  // #endregion

  return {
    onImportOutboxFromTxt,
    importFromTxtLable,
    importFromTxtTooltip
  }
}
